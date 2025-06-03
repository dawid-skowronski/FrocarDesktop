using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.ComponentModel;
using AdminPanel.Services;
using AdminPanel.Models;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace AdminPanel.Views
{
    public partial class MainWindow : Window
    {
        private TrayIcon? _trayIcon;
        private NativeMenu? _trayMenu;
        private NotificationService? _notificationService;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;

            _trayIcon = new TrayIcon();

            var iconPath = OperatingSystem.IsWindows()
                ? "avares://AdminPanel/Assets/frocar.ico"
                : "avares://AdminPanel/Assets/icon.png";
            try
            {
                _trayIcon.Icon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B³¹d ³adowania ikony tray'a: {ex.Message}");
            }

            _trayIcon.ToolTipText = "Frocar Admin Panel";

            _trayMenu = CreateTrayMenu();
            _trayIcon.Menu = _trayMenu;

            _trayIcon.Clicked += (sender, args) =>
            {
                if (this.WindowState == WindowState.Minimized || !this.IsVisible)
                {
                    RestoreFromTray();
                }
            };

            _trayIcon.IsVisible = true;

            _notificationService = new NotificationService();
            _notificationService.Start(async notifications =>
            {
                foreach (var notification in notifications)
                {
                    await ShowNotificationPopup(notification);
                    await Task.Delay(200);
                }
            });

            Closing += OnWindowClosing;
        }

        private async Task ShowNotificationPopup(NotificationDto notification)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var popup = new NotificationPopup(notification, _notificationService);
                popup.Show();
            }, DispatcherPriority.Render);
        }

        private NativeMenu CreateTrayMenu()
        {
            var menu = new NativeMenu();

            var restoreItem = new NativeMenuItem("Przywróæ");
            restoreItem.Click += (sender, args) => RestoreFromTray();
            menu.Add(restoreItem);

            menu.Add(new NativeMenuItemSeparator());

            var exitItem = new NativeMenuItem("Zamknij");
            exitItem.Click += (sender, args) => CloseApplication();
            menu.Add(exitItem);

            return menu;
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void CloseApplication()
        {
            Closing -= OnWindowClosing;
            _notificationService?.Stop();
            NotificationManager.CloseAllNotifications();
            if (_trayIcon != null)
            {
                _trayIcon.IsVisible = false;
                _trayIcon.Dispose();
            }
            this.Close();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (_trayIcon != null)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                this.Hide();
            }
        }
    }
}