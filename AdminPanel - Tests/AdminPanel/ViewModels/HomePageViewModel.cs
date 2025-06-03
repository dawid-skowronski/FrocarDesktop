using ReactiveUI;
using System.Reactive;
using AdminPanel.Services;
using AdminPanel.Views;

namespace AdminPanel.ViewModels
{
    
    public class HomePageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }

        public HomePageViewModel() : this(new NavigationService(new DefaultWindowProvider()))
        {
        }

        public HomePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoginCommand = ReactiveCommand.Create(NavigateToLogin);
            RegisterCommand = ReactiveCommand.Create(NavigateToRegister);
        }

        private void NavigateToLogin()
        {
            _navigationService.NavigateTo<LoginPage>(new LoginPageViewModel(_navigationService));
        }

        private void NavigateToRegister()
        {
            _navigationService.NavigateTo<RegisterPage>(new RegisterPageViewModel(_navigationService));
        }
    }
}