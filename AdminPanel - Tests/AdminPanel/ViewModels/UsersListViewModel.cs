﻿using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Models;
using AdminPanel.Services;
using System.Linq;
using System;
using Avalonia.Controls;
using Avalonia.Styling;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public partial class UsersListViewModel : ViewModelBase
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

        public static Func<string, string, Task>? ShowMessageBoxDelegate { get; set; }
        public static Func<int, Task<bool>>? ShowConfirmDeleteDialogDelegate { get; set; }

        public UsersListViewModel()
        {
            RefreshCommand = ReactiveCommand.CreateFromTask(GetUsersAsync);
            SearchCommand = ReactiveCommand.Create(SearchUsers);
            ResetSearchCommand = ReactiveCommand.Create(ResetSearch);

            _ = GetUsersAsync();
        }

        private async Task GetUsersAsync()
        {
            var result = await UserService.GetUsers();
            if (result.IsSuccess)
            {
                _allUsers.Clear();
                Users.Clear();
                int currentUserId = TokenService.GetUserId();
                foreach (var user in result.Users)
                {
                    user.IsCurrentUser = user.Id == currentUserId;
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

            var query = SearchQuery.Trim().ToLowerInvariant();
            var filtered = _allUsers.Where(user =>
                user.Id.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (user.Username != null && user.Username.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (user.Email != null && user.Email.Contains(query, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            Users.Clear();
            foreach (var user in filtered)
            {
                Users.Add(user);
            }
        }

        private void ResetSearch()
        {
            this.RaiseAndSetIfChanged(ref _searchQuery, "", nameof(SearchQuery)); // Jawne wywołanie RaiseAndSetIfChanged
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

                await GetUsersAsync();
            }
            catch (Exception ex)
            {
                await ShowMessageBox("Błąd", $"Błąd podczas edycji użytkownika: {ex.Message}");
            }
        }

        private async Task ConfirmDeleteUser(UserDto user)
        {
            if (user.Id == TokenService.GetUserId())
            {
                await ShowMessageBox("Błąd", "Nie możesz usunąć własnego konta!");
                return;
            }

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
                var (isSuccess, errorMessage) = await UserService.DeleteUser(user.Id);
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
            if (ShowConfirmDeleteDialogDelegate != null)
            {
                return await ShowConfirmDeleteDialogDelegate(userId);
            }

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
            if (ShowMessageBoxDelegate != null)
            {
                await ShowMessageBoxDelegate(title, message);
                return;
            }

            var dialog = new MessageBoxView(title)
            {
                Message = message,
                RequestedThemeVariant = App.MainWindow?.RequestedThemeVariant ?? ThemeVariant.Default
            };
            await dialog.ShowDialog(App.MainWindow);
        }
    }
}