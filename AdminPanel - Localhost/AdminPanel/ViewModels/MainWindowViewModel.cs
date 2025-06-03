using ReactiveUI;

namespace AdminPanel.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;
        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public MainWindowViewModel()
        {
            CurrentPage = new LoginPageViewModel();
        }

        public void NavigateToDashboard()
        {
            CurrentPage = new DashboardPageViewModel();
        }
    }
}
