using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Services;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public class RegisterPageViewModel : ReactiveObject
    {
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;

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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
        public ReactiveCommand<Unit, Unit> GoBackToHomeCommand { get; }

        public RegisterPageViewModel()
        {
            RegisterCommand = ReactiveCommand.CreateFromTask(RegisterAsync);
            GoBackToHomeCommand = ReactiveCommand.Create(GoBackToHome);
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Wszystkie pola są wymagane.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Hasła się nie zgadzają.";
                return;
            }

            var (isSuccess, message) = await UserService.Register(Username, Email, Password, ConfirmPassword);
            if (isSuccess)
            {
                App.MainWindow.Content = new LoginPage
                {
                    DataContext = new LoginPageViewModel()
                };
            }
            else
            {
                ErrorMessage = message;
            }
        }
        private void GoBackToHome()
        {
            App.MainWindow.Content = new HomePage
            {
                DataContext = new HomePageViewModel()
            };
        }
    }
}
