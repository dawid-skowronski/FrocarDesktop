using ReactiveUI;
using System.Reactive;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetPasswordCommand { get; }

        public HomePageViewModel()
        {
            LoginCommand = ReactiveCommand.Create(NavigateToLogin);
            RegisterCommand = ReactiveCommand.Create(NavigateToRegister);
            ResetPasswordCommand = ReactiveCommand.Create(NavigateToPasswordReset);
        }

        private void NavigateToLogin()
        {
            App.MainWindow.Content = new LoginPage
            {
                DataContext = new LoginPageViewModel()
            };
        }

        private void NavigateToRegister()
        {
            App.MainWindow.Content = new RegisterPage
            {
                DataContext = new RegisterPageViewModel()
            };
        }

        private void NavigateToPasswordReset()
        {
            App.MainWindow.Content = new PasswordResetPage
            {
                DataContext = new PasswordResetPageViewModel()
            };
        }
    }
}