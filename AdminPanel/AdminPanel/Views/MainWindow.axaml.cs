using Avalonia.Controls;
using Avalonia.Platform;
using System;
using System.ComponentModel;

namespace AdminPanel.Views
{
    public partial class MainWindow : Window
    {
        private TrayIcon? _trayIcon;
        private NativeMenu? _trayMenu;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;

            // Inicjalizacja tray'a systemowego
            _trayIcon = new TrayIcon();

            // Ustawienie ikony tray'a
            var iconPath = OperatingSystem.IsWindows()
                ? "avares://AdminPanel/Assets/frocar.ico" // Ikona dla Windows
                : "avares://AdminPanel/Assets/icon.png"; // PNG dla macOS/Linux, je�li istnieje
            try
            {
                _trayIcon.Icon = new WindowIcon(AssetLoader.Open(new Uri(iconPath)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B��d �adowania ikony tray'a: {ex.Message}");
            }

            // Ustawienie tooltipa
            _trayIcon.ToolTipText = "Frocar Admin Panel";

            // Tworzenie menu kontekstowego
            _trayMenu = CreateTrayMenu();
            _trayIcon.Menu = _trayMenu;

            // Obs�uga klikni�cia LPM na ikonie tray'a
            _trayIcon.Clicked += (sender, args) =>
            {
                if (this.WindowState == WindowState.Minimized || !this.IsVisible)
                {
                    RestoreFromTray();
                }
            };

            // Ustawienie ikony jako widocznej
            _trayIcon.IsVisible = true;

            // Obs�uga zamykania okna
            Closing += OnWindowClosing;
        }

        private NativeMenu CreateTrayMenu()
        {
            var menu = new NativeMenu();

            // Opcja "Przywr��"
            var restoreItem = new NativeMenuItem("Przywr��");
            restoreItem.Click += (sender, args) => RestoreFromTray();
            menu.Add(restoreItem);

            // Separator
            menu.Add(new NativeMenuItemSeparator());

            // Opcja "Zamknij"
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
            // Prawdziwe zamkni�cie aplikacji
            Closing -= OnWindowClosing; // Usu� obs�ug� zamykania, �eby nie zminimalizowa� ponownie
            if (_trayIcon != null)
            {
                _trayIcon.IsVisible = false; // Ukryj ikon� tray'a
                _trayIcon.Dispose(); // Zwolnij zasoby
            }
            this.Close();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Zamiast zamyka�, minimalizujemy do tray'a
            if (_trayIcon != null)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
                this.Hide();
            }
        }
    }
}