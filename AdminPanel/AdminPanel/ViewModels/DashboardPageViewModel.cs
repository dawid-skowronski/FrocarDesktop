using ReactiveUI;
using Avalonia.Controls;
using System.Reactive;
using AdminPanel.Views;
using Splat;
using AdminPanel.Services;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia;

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

        // Właściwości do wskazywania aktywnego widoku
        private bool _isHomeView;
        private bool _isUsersListView;
        private bool _isCreateCarView;
        private bool _isCarsListView;
        private bool _isCarMapView;
        private bool _isRentalsListView;
        private bool _isCarsToApproveView;
        private bool _isStatisticsView;

        public bool IsHomeView
        {
            get => _isHomeView;
            set => this.RaiseAndSetIfChanged(ref _isHomeView, value);
        }
        public bool IsUsersListView
        {
            get => _isUsersListView;
            set => this.RaiseAndSetIfChanged(ref _isUsersListView, value);
        }
        public bool IsCreateCarView
        {
            get => _isCreateCarView;
            set => this.RaiseAndSetIfChanged(ref _isCreateCarView, value);
        }
        public bool IsCarsListView
        {
            get => _isCarsListView;
            set => this.RaiseAndSetIfChanged(ref _isCarsListView, value);
        }
        public bool IsCarMapView
        {
            get => _isCarMapView;
            set => this.RaiseAndSetIfChanged(ref _isCarMapView, value);
        }
        public bool IsRentalsListView
        {
            get => _isRentalsListView;
            set => this.RaiseAndSetIfChanged(ref _isRentalsListView, value);
        }
        public bool IsCarsToApproveView
        {
            get => _isCarsToApproveView;
            set => this.RaiseAndSetIfChanged(ref _isCarsToApproveView, value);
        }
        public bool IsStatisticsView
        {
            get => _isStatisticsView;
            set => this.RaiseAndSetIfChanged(ref _isStatisticsView, value);
        }

        public ReactiveCommand<Unit, Unit> ShowHomePageCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowUsersListCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCreateCarCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCarsListCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCarMapCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowRentalsListCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowCarsToApproveCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowStatisticsCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleThemeCommand { get; }

        public DashboardPageViewModel()
        {
            // Domyślny widok po zalogowaniu
            CurrentView = new HomePageAdmin();
            IsHomeView = true;

            // Inicjalizacja komend
            ShowHomePageCommand = ReactiveCommand.Create(() => ChangeView(new HomePageAdmin(), nameof(IsHomeView)));
            ShowUsersListCommand = ReactiveCommand.Create(() => ChangeView(new UsersList(), nameof(IsUsersListView)));
            ShowCreateCarCommand = ReactiveCommand.Create(() => ChangeView(new CreateCar(), nameof(IsCreateCarView)));
            ShowCarsListCommand = ReactiveCommand.Create(() => ChangeView(new CarsList(), nameof(IsCarsListView)));
            ShowCarMapCommand = ReactiveCommand.Create(() => ChangeView(new CarMapView(), nameof(IsCarMapView)));
            ShowRentalsListCommand = ReactiveCommand.Create(() => ChangeView(new RentalsList(), nameof(IsRentalsListView)));
            ShowCarsToApproveCommand = ReactiveCommand.Create(() => ChangeView(new CarsToApprove(), nameof(IsCarsToApproveView)));
            ShowStatisticsCommand = ReactiveCommand.Create(() => ChangeView(new StatisticsView(), nameof(IsStatisticsView)));
            LogoutCommand = ReactiveCommand.Create(Logout);
            ToggleThemeCommand = ReactiveCommand.Create(ToggleTheme);
        }

        // Metoda zmieniająca widok i aktualizująca aktywny stan
        private void ChangeView(UserControl newView, string activeViewProperty)
        {
            CurrentView = newView;
            // Resetuj wszystkie właściwości
            IsHomeView = false;
            IsUsersListView = false;
            IsCreateCarView = false;
            IsCarsListView = false;
            IsCarMapView = false;
            IsRentalsListView = false;
            IsCarsToApproveView = false;
            IsStatisticsView = false;
            // Ustaw odpowiednią właściwość na true
            switch (activeViewProperty)
            {
                case nameof(IsHomeView): IsHomeView = true; break;
                case nameof(IsUsersListView): IsUsersListView = true; break;
                case nameof(IsCreateCarView): IsCreateCarView = true; break;
                case nameof(IsCarsListView): IsCarsListView = true; break;
                case nameof(IsCarMapView): IsCarMapView = true; break;
                case nameof(IsRentalsListView): IsRentalsListView = true; break;
                case nameof(IsCarsToApproveView): IsCarsToApproveView = true; break;
                case nameof(IsStatisticsView): IsStatisticsView = true; break;
            }
        }

        private void Logout()
        {
            TokenService.ClearToken();
            App.MainWindow.Content = new HomePage
            {
                DataContext = new HomePageViewModel()
            };
        }

        private void ToggleTheme()
        {
            if (Application.Current != null)
            {
                var currentTheme = Application.Current.RequestedThemeVariant;
                Application.Current.RequestedThemeVariant = currentTheme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
            }
        }
    }
}