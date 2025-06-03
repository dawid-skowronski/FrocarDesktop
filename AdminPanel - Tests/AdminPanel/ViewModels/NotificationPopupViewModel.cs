using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.Views;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace AdminPanel.ViewModels
{
    public class NotificationPopupViewModel : ReactiveObject
    {
        private readonly NotificationDto _notification;
        private readonly NotificationPopup _window;
        private readonly NotificationService _notificationService;

        public string Message => _notification.Message;
        public ReactiveCommand<Unit, Unit> MarkAsReadCommand { get; }
        public ReactiveCommand<Unit, Unit> MarkAllAsReadCommand { get; }

        public NotificationPopupViewModel(NotificationDto notification, NotificationPopup window, NotificationService notificationService)
        {
            _notification = notification;
            _window = window;
            _notificationService = notificationService;

            MarkAsReadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await _notificationService.MarkAsRead(_notification.NotificationId);
                if (result.IsSuccess)
                {
                    await _window.SlideOutAndClose();
                }
            });

            MarkAllAsReadCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await _notificationService.MarkAllAsRead();
                if (result.IsSuccess)
                {
                    await NotificationManager.CloseAllNotifications();
                }
            });
        }

        public int GetNotificationId() => _notification.NotificationId;
    }
}