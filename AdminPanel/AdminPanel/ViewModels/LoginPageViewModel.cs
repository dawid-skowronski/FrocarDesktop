using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Services;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        //private string _username = string.Empty;
        //private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        private string _username = "dawid";                 //usun to to tylko do testow :PPPP
        private string _password = "Dawid123!";

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

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> GoBackToHomeCommand { get; }

        public LoginPageViewModel()
        {
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            GoBackToHomeCommand = ReactiveCommand.Create(GoBackToHome);
        }


        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

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
