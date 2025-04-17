using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using System.Linq;
using System;
using Avalonia.Controls;
using AdminPanel.Views;
using Avalonia.Styling;

namespace AdminPanel.ViewModels
{
    public class UsersListViewModel : ViewModelBase
    {
        private ObservableCollection<UserDto> _users = new();
        private ObservableCollection<UserDto> _allUsers = new();
        private string _searchQuery = "";

        public ObservableCollection<UserDto> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> SearchCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetSearchCommand { get; }

        public UsersListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(GetUsersAsync);
            SearchCommand = ReactiveCommand.Create(SearchUsers);
            ResetSearchCommand = ReactiveCommand.Create(ResetSearch);

            _ = GetUsersAsync();
        }

        private async Task GetUsersAsync()
        {
            var result = await ApiService.GetUsers();
            if (result.IsSuccess)
            {
                _allUsers.Clear();
                Users.Clear();
                foreach (var user in result.Users)
                {
                    user.EditCommand = ReactiveCommand.CreateFromTask<UserDto>(EditUser);
                    user.DeleteCommand = ReactiveCommand.CreateFromTask<UserDto>(ConfirmDeleteUser);
                    _allUsers.Add(user);
                    Users.Add(user);
                }
            }
        }

        private void SearchUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                ResetSearch();
                return;
            }

            var query = SearchQuery.Trim().ToLower();
            var filtered = _allUsers.Where(user =>
                user.Id.ToString().Contains(query) ||
                (user.Username?.ToLower().Contains(query) ?? false) ||
                (user.Email?.ToLower().Contains(query) ?? false)
            ).ToList();

            Users.Clear();
            foreach (var user in filtered)
            {
                Users.Add(user);
            }
        }

        private void ResetSearch()
        {
            SearchQuery = "";
            Users.Clear();
            foreach (var user in _allUsers)
            {
                Users.Add(user);
            }
        }

        private async Task EditUser(UserDto user)
        {
            try
            {
                var dialog = new EditUserDialog();
                dialog.DataContext = new EditUserDialogViewModel(user, dialog);
                await dialog.ShowDialog(App.MainWindow);

                // Odśwież listę po zamknięciu dialogu
                await GetUsersAsync();
            }
            catch (Exception ex)
            {
                await ShowMessageBox("Błąd", $"Błąd podczas edycji użytkownika: {ex.Message}");
            }
        }

        private async Task ConfirmDeleteUser(UserDto user)
        {
            try
            {
                bool confirmed = await ShowConfirmDeleteDialog(user.Id);
                if (confirmed)
                {
                    await DeleteUser(user);
                }
            }
            catch (Exception ex)
            {
                await ShowMessageBox("Błąd", $"Błąd podczas usuwania użytkownika: {ex.Message}");
            }
        }

        private async Task DeleteUser(UserDto user)
        {
            try
            {
                var (isSuccess, errorMessage) = await ApiService.DeleteUser(user.Id);
                if (isSuccess)
                {
                    _allUsers.Remove(user);
                    Users.Remove(user);
                    await ShowMessageBox("Sukces", "Użytkownik został usunięty!");
                }
                else
                {
                    await ShowMessageBox("Błąd", errorMessage ?? "Nie udało się usunąć użytkownika.");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageBox("Błąd", $"Błąd podczas usuwania użytkownika: {ex.Message}");
            }
        }

        private async Task<bool> ShowConfirmDeleteDialog(int userId)
        {
            var dialog = new ConfirmMessageBoxView("Potwierdzenie usunięcia")
            {
                Message = $"Czy na pewno chcesz usunąć użytkownika o ID {userId}?",
                RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
            };
            await dialog.ShowDialog(App.MainWindow);
            return await dialog.Result.Task;
        }

        private async Task ShowMessageBox(string title, string message)
        {
            var dialog = new MessageBoxView(title)
            {
                Message = message,
                RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
            };
            await dialog.ShowDialog(App.MainWindow);
        }
    }
}