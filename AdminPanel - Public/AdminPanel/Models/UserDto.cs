using System.Reactive;
using ReactiveUI;

namespace AdminPanel.Models
{
    public class UserDto : ReactiveObject
    {
        private bool _isCurrentUser;

        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }

        public bool IsCurrentUser
        {
            get => _isCurrentUser;
            set => this.RaiseAndSetIfChanged(ref _isCurrentUser, value);
        }

        public ReactiveCommand<UserDto, Unit> EditCommand { get; set; }
        public ReactiveCommand<UserDto, Unit> DeleteCommand { get; set; }
    }
}