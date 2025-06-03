using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using AdminPanel.Services;
using AdminPanel.Views;
using Avalonia.Controls;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("AdminPanel.Tests")]

namespace AdminPanel.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private readonly INavigationService _navigationService;

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

        public LoginPageViewModel() : this(new NavigationService(new DefaultWindowProvider()))
        {
        }

        public LoginPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            GoBackToHomeCommand = ReactiveCommand.Create(GoBackToHome);
        }

        internal async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Wszystkie pola są wymagane.";
                return;
            }

            var result = await UserService.Login(Username, Password);
            if (result.IsSuccess)
            {
                _navigationService.NavigateTo<DashboardPage>(new DashboardPageViewModel());
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }

        internal void GoBackToHome()
        {
            _navigationService.NavigateTo<HomePageView>(new HomePageViewModel());
        }
    }

    internal class DefaultWindowProvider : IWindowProvider
    {
        public Window MainWindow
        {
            get => App.MainWindow;
        }
    }
}