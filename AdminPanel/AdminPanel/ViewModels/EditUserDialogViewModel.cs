using AdminPanel.Models;
using AdminPanel.Services;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace AdminPanel.ViewModels
{
    public class EditUserDialogViewModel : ViewModelBase
    {
        private UserDto _user;
        private string _username;
        private string _email;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private string _title;
        private readonly Window _dialog;

        public UserDto User
        {
            get => _user;
            set => this.RaiseAndSetIfChanged(ref _user, value);
        }

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }

        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public EditUserDialogViewModel(UserDto user, Window dialog)
        {
            _dialog = dialog;
            User = user;

            Username = user.Username;
            Email = user.Email;
            Title = $"Edycja użytkownika {user.Username} (ID: {user.Id})";

            SaveCommand = ReactiveCommand.CreateFromTask(Save);
            CancelCommand = ReactiveCommand.Create(() => _dialog.Close());
        }

        private async Task Save()
        {
            try
            {
                ErrorMessage = "";

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Nazwa użytkownika jest wymagana.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ErrorMessage = "Email jest wymagany.";
                    return;
                }

                var (isSuccess, message) = await UserService.UpdateUser(User.Id, Username, Email, Password);
                if (isSuccess)
                {
                    User.Username = Username;
                    User.Email = Email;
                    _dialog.Close();
                }
                else
                {
                    ErrorMessage = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Błąd podczas zapisywania: {ex.Message}";
            }
        }
    }
}