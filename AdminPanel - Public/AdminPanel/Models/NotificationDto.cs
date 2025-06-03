using ReactiveUI;
using System;

namespace AdminPanel.Models
{
    public class NotificationDto : ReactiveObject
    {
        private int _notificationId;
        private int _userId;
        private string? _message;
        private DateTime _createdAt;
        private bool _isRead;

        public int NotificationId
        {
            get => _notificationId;
            set => this.RaiseAndSetIfChanged(ref _notificationId, value);
        }

        public int UserId
        {
            get => _userId;
            set => this.RaiseAndSetIfChanged(ref _userId, value);
        }

        public UserDto? User { get; set; }

        public string? Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        public bool IsRead
        {
            get => _isRead;
            set => this.RaiseAndSetIfChanged(ref _isRead, value);
        }
    }
}