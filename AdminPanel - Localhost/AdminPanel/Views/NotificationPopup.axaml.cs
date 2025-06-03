using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPanel.Views
{
    public partial class NotificationPopup : Window
    {
        private readonly NotificationDto _notification;
        private readonly NotificationService _notificationService;
        private readonly TranslateTransform _slideTransform;
        private bool _isPositioned;

        public NotificationPopup()
        {
            InitializeComponent();
        }

        public NotificationPopup(NotificationDto notification, NotificationService notificationService)
        {
            InitializeComponent();
            _notification = notification;
            _notificationService = notificationService;
            DataContext = new NotificationPopupViewModel(notification, this, notificationService);

            _slideTransform = new TranslateTransform { X = 400 };
            RenderTransform = _slideTransform;
            Opacity = 0;

            IsVisible = true;

            NotificationManager.AddNotification(this);

            LayoutUpdated += OnLayoutUpdated;
        }

        private void InitializeComponent()
        {
            try
            {
                AvaloniaXamlLoader.Load(this);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"B³¹d w InitializeComponent: {ex.Message}");
            }
        }

        private async void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (_isPositioned) return;
            _isPositioned = true;

            if (Bounds.Height <= 0)
            {
                await Task.Delay(50);
                PositionWindow();
                await RunSlideInAnimation();
                return;
            }

            PositionWindow();
            await RunSlideInAnimation();
        }

        private void PositionWindow()
        {
            if (Screens.Primary == null)
            {
                Position = new PixelPoint(100, 100);
                return;
            }

            var screen = Screens.Primary.WorkingArea;
            var index = NotificationManager.GetNotificationIndex(this);
            var height = Math.Max((int)Bounds.Height, 80);
            var newPosition = new PixelPoint(
                screen.BottomRight.X - (int)Width - 10,
                screen.BottomRight.Y - height - 10 - index * (height + 5));

            if (newPosition.Y < screen.Y)
            {
                newPosition = new PixelPoint(newPosition.X, screen.Y + 10);
            }
            Position = newPosition;
        }

        private async Task RunSlideInAnimation()
        {
            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                Easing = new QuadraticEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 400.0),
                            new Setter(OpacityProperty, 0.0)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0.0),
                            new Setter(OpacityProperty, 1.0)
                        }
                    }
                }
            };

            await animation.RunAsync(this);
        }

        public async Task SlideOutAndClose()
        {
            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                Easing = new QuadraticEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 0.0),
                            new Setter(OpacityProperty, 1.0)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters =
                        {
                            new Setter(TranslateTransform.XProperty, 400.0),
                            new Setter(OpacityProperty, 0.0)
                        }
                    }
                }
            };

            await animation.RunAsync(this);
            NotificationManager.RemoveNotification(this);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }

    public static class NotificationManager
    {
        private static readonly List<NotificationPopup> _activeNotifications = new List<NotificationPopup>();

        public static void AddNotification(NotificationPopup notification)
        {
            _activeNotifications.Add(notification);
            UpdatePositions();
        }

        public static void RemoveNotification(NotificationPopup notification)
        {
            _activeNotifications.Remove(notification);
            UpdatePositions();
        }

        public static int GetNotificationIndex(NotificationPopup notification)
        {
            var index = _activeNotifications.IndexOf(notification);
            return index;
        }

        public static async Task CloseAllNotifications()
        {
            var notifications = _activeNotifications.ToList();
            var closeTasks = notifications.Select(async notification =>
            {
                await notification.SlideOutAndClose();
            }).ToList();

            await Task.WhenAll(closeTasks);
            _activeNotifications.Clear();
        }

        private static void UpdatePositions()
        {
            var referenceWindow = _activeNotifications.FirstOrDefault();
            if (referenceWindow == null || referenceWindow.Screens.Primary == null)
            {
                return;
            }

            var screen = referenceWindow.Screens.Primary.WorkingArea;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                for (int i = 0; i < _activeNotifications.Count; i++)
                {
                    var notification = _activeNotifications[i];
                    if (!notification.IsVisible)
                    {
                        continue;
                    }

                    var height = Math.Max((int)notification.Bounds.Height, 80);
                    var newPosition = new PixelPoint(
                        screen.BottomRight.X - (int)notification.Width - 10,
                        screen.BottomRight.Y - height - 10 - i * (height + 5));

                    if (newPosition.Y < screen.Y)
                    {
                        newPosition = new PixelPoint(newPosition.X, screen.Y + 10);
                    }

                    notification.Position = newPosition;
                }
            }, DispatcherPriority.Render);
        }

        public static int GetNotificationId(this NotificationPopup notification)
        {
            if (notification.DataContext is NotificationPopupViewModel vm)
            {
                return vm.GetNotificationId();
            }
            return -1;
        }
    }
}