using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;

namespace AdminPanel.ViewModels
{
    public class UsersListViewModel : ViewModelBase
    {
        public ObservableCollection<UserDto> Users { get; } = new();

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public UsersListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(GetUsersAsync);
            _ = GetUsersAsync(); // Pobierz użytkowników na start
        }

        private async Task GetUsersAsync()
        {
            var result = await ApiService.GetUsers();
            if (result.IsSuccess)
            {
                Users.Clear();
                foreach (var user in result.Users)
                {
                    Users.Add(user);
                }
            }
        }
    }
}
