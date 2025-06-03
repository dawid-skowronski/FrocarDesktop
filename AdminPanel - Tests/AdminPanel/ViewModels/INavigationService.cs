using Avalonia.Controls;

namespace AdminPanel.ViewModels
{
    public interface INavigationService
    {
        void NavigateTo<TView>(object viewModel) where TView : Control, new();
    }

    public class NavigationService : INavigationService
    {
        private readonly IWindowProvider _windowProvider;

        public NavigationService(IWindowProvider windowProvider)
        {
            _windowProvider = windowProvider;
        }

        public void NavigateTo<TView>(object viewModel) where TView : Control, new()
        {
            _windowProvider.MainWindow.Content = new TView
            {
                DataContext = viewModel
            };
        }
    }
}