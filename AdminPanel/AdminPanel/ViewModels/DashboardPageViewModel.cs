using ReactiveUI;
using Avalonia.Controls;
using System.Reactive;
using AdminPanel.Views;
using Splat;
using AdminPanel.Services;

namespace AdminPanel.ViewModels
{
    public class DashboardPageViewModel : ViewModelBase
    {
        private UserControl _currentView;
        public UserControl CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public ReactiveCommand<Unit, Unit> ShowHomePageCommand { get; }
        //users
        public ReactiveCommand<Unit, Unit> ShowUsersListCommand { get; }
        //cars
        public ReactiveCommand<Unit, Unit> ShowCreateCarCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCarsListCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCarMapCommand { get; } // Nowa komenda

        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }

        public DashboardPageViewModel()
        {
            // Domyślny widok po zalogowaniu
            CurrentView = new HomePageAdmin();

            // Poprawione komendy
            ShowHomePageCommand = ReactiveCommand.Create(() => ChangeView(new HomePageAdmin()));
            ShowUsersListCommand = ReactiveCommand.Create(() => ChangeView(new UsersList()));
            ShowCreateCarCommand = ReactiveCommand.Create(() => ChangeView(new CreateCar()));
            ShowCarsListCommand = ReactiveCommand.Create(() => ChangeView(new CarsList()));
            ShowCarMapCommand = ReactiveCommand.Create(() => ChangeView(new CarMapView())); // Obsługa nowego widoku
            LogoutCommand = ReactiveCommand.Create(Logout);
        }

        // Metoda zmieniająca widok
        private void ChangeView(UserControl newView)
        {
            CurrentView = newView;
        }

        private void Logout()
        {
            TokenService.ClearToken();
            App.MainWindow.Content = new HomePage
            {
                DataContext = new HomePageViewModel()
            };
        }
    }
}