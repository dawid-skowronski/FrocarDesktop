using AdminPanel.ViewModels;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public class MainWindowViewModelTests : TestBase
    {
        [Fact]
        public void Constructor_SetsCurrentPageToLoginPageViewModel()
        {
            // Arrange
            var viewModel = new MainWindowViewModel();

            // Act
            var currentPage = viewModel.CurrentPage;

            // Assert
            Assert.NotNull(currentPage);
            Assert.IsType<LoginPageViewModel>(currentPage);
        }

        [Fact]
        public void NavigateToDashboard_SetsCurrentPageToDashboardPageViewModel()
        {
            // Arrange
            var viewModel = new MainWindowViewModel();

            // Act
            viewModel.NavigateToDashboard();
            var currentPage = viewModel.CurrentPage;

            // Assert
            Assert.NotNull(currentPage);
            Assert.IsType<DashboardPageViewModel>(currentPage);
        }

        [Fact]
        public void CurrentPage_PropertyChangeNotification()
        {
            // Arrange
            var viewModel = new MainWindowViewModel();
            bool propertyChangedFired = false;
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MainWindowViewModel.CurrentPage))
                {
                    propertyChangedFired = true;
                }
            };

            // Act
            viewModel.NavigateToDashboard();

            // Assert
            Assert.True(propertyChangedFired);
        }
    }
}