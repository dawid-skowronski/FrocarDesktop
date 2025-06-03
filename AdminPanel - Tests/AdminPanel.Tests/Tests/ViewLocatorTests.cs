using AdminPanel;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using Avalonia.Controls;
using System.Diagnostics;
using Xunit;

namespace AdminPanel.Tests
{
    public class ViewLocatorTests : TestBase
    {
        [Fact]
        public void Build_ReturnsControl_WhenTypeExists()
        {
            // Arrange
            var viewLocator = new ViewLocator();
            var viewModel = new HomePageViewModel();
            Debug.WriteLine($"Testing ViewLocator with ViewModel: {viewModel.GetType().FullName}");

            // Act
            var result = viewLocator.Build(viewModel);

            // Assert
            Assert.NotNull(result);
            Debug.WriteLine($"Result type: {result.GetType().FullName}");
            Assert.IsType<HomePageView>(result);
        }

        [Fact]
        public void Build_ReturnsTextBlock_WhenTypeDoesNotExist()
        {
            // Arrange
            var viewLocator = new ViewLocator();
            var fakeViewModel = new FakeViewModel();

            // Act
            var result = viewLocator.Build(fakeViewModel);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TextBlock>(result);
            Assert.Equal("Not Found: AdminPanel.Tests.FakeView", ((TextBlock)result).Text);
        }

        [Fact]
        public void Match_ReturnsTrue_WhenDataIsViewModelBase()
        {
            // Arrange
            var viewLocator = new ViewLocator();
            var viewModel = new HomePageViewModel();

            // Act
            var result = viewLocator.Match(viewModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Match_ReturnsFalse_WhenDataIsNull()
        {
            // Arrange
            var viewLocator = new ViewLocator();

            // Act
            var result = viewLocator.Match(null);

            // Assert
            Assert.False(result);
        }
    }

    public class FakeViewModel : ViewModelBase
    {
    }
}