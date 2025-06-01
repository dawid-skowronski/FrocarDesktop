using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Services;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public class PasswordResetPageViewModel : ViewModelBase
    {
        private string _email = string.Empty;
        private string _errorMessage = string.Empty;

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ReactiveCommand<Unit, Unit> RequestPasswordResetCommand { get; }
        public ReactiveCommand<Unit, Unit> GoBackToHomeCommand { get; }

        public PasswordResetPageViewModel()
        {
            RequestPasswordResetCommand = ReactiveCommand.CreateFromTask(RequestPasswordReset);
            GoBackToHomeCommand = ReactiveCommand.Create(GoBackToHome);
        }

        private async Task RequestPasswordReset()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Proszę wpisać adres e-mail";
                return;
            }

            var (isSuccess, message) = await UserService.RequestPasswordReset(Email);
            ErrorMessage = message;

            if (isSuccess)
            {
                GoBackToHome();
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