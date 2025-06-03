using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Services;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _showPassword;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
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

        public bool ShowPassword
        {
            get => _showPassword;
            set => this.RaiseAndSetIfChanged(ref _showPassword, value);
        }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> GoBackToHomeCommand { get; }
        public ReactiveCommand<Unit, Unit> TogglePasswordCommand { get; }

        public LoginPageViewModel()
        {
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            GoBackToHomeCommand = ReactiveCommand.Create(GoBackToHome);
            TogglePasswordCommand = ReactiveCommand.Create(TogglePassword);
        }

        private void TogglePassword()
        {
            ShowPassword = !ShowPassword;
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Nazwa użytkownika nie może być pusta.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Hasło nie może być puste.";
                return;
            }

            var result = await UserService.Login(Username, Password);
            if (result.IsSuccess)
            {
                App.MainWindow.Content = new DashboardPage
                {
                    DataContext = new DashboardPageViewModel()
                };
            }
            else
            {
                ErrorMessage = result.Message;
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