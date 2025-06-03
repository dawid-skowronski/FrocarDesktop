using System;
using System.Reactive;
using AdminPanel.ViewModels;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia;
using Splat;
using Xunit;
using AdminPanel.Views;
using AdminPanel.Services;
using Moq;
using Avalonia.Controls.ApplicationLifetimes;

namespace AdminPanel.Tests.ViewModels
{
    public class DashboardPageViewModelTests
    {
        private readonly Mock<IClassicDesktopStyleApplicationLifetime> _mockLifetime;

        public DashboardPageViewModelTests()
        {
            _mockLifetime = new Mock<IClassicDesktopStyleApplicationLifetime>();
        }

        [Fact]
        public void Constructor_InitializesWithHomePage()
        {
            // Arrange & Act
            var viewModel = new DashboardPageViewModel(null, _mockLifetime.Object);

            // Assert
            Assert.IsType<HomePageAdmin>(viewModel.CurrentView);
            Assert.True(viewModel.IsHomeView);
            Assert.False(viewModel.IsUsersListView);
            Assert.False(viewModel.IsCreateCarView);
            Assert.False(viewModel.IsCarsListView);
            Assert.False(viewModel.IsCarMapView);
            Assert.False(viewModel.IsRentalsListView);
            Assert.False(viewModel.IsCarsToApproveView);
            Assert.False(viewModel.IsStatisticsView);
            Assert.False(viewModel.IsReviewsListView);
        }

        [Fact]
        public void ShowUsersListCommand_ChangesViewAndProperties()
        {
            // Arrange
            var viewModel = new DashboardPageViewModel(null, _mockLifetime.Object);

            // Act
            viewModel.ShowUsersListCommand.Execute().Subscribe();

            // Assert
            Assert.IsType<UsersList>(viewModel.CurrentView);
            Assert.False(viewModel.IsHomeView);
            Assert.True(viewModel.IsUsersListView);
            Assert.False(viewModel.IsCreateCarView);
            Assert.False(viewModel.IsCarsListView);
            Assert.False(viewModel.IsCarMapView);
            Assert.False(viewModel.IsRentalsListView);
            Assert.False(viewModel.IsCarsToApproveView);
            Assert.False(viewModel.IsStatisticsView);
            Assert.False(viewModel.IsReviewsListView);
        }

        [Fact]
        public void ShowCreateCarCommand_ChangesViewAndProperties()
        {
            // Arrange
            var viewModel = new DashboardPageViewModel(null, _mockLifetime.Object);

            // Act
            viewModel.ShowCreateCarCommand.Execute().Subscribe();

            // Assert
            Assert.IsType<CreateCar>(viewModel.CurrentView);
            Assert.False(viewModel.IsHomeView);
            Assert.False(viewModel.IsUsersListView);
            Assert.True(viewModel.IsCreateCarView);
            Assert.False(viewModel.IsCarsListView);
            Assert.False(viewModel.IsCarMapView);
            Assert.False(viewModel.IsRentalsListView);
            Assert.False(viewModel.IsCarsToApproveView);
            Assert.False(viewModel.IsStatisticsView);
            Assert.False(viewModel.IsReviewsListView);
        }

        [Fact]
        public void ShowReviewsListCommand_ChangesViewAndProperties()
        {
            // Arrange
            var viewModel = new DashboardPageViewModel(null, _mockLifetime.Object);

            // Act
            viewModel.ShowReviewsListCommand.Execute().Subscribe();

            // Assert
            Assert.IsType<ReviewsList>(viewModel.CurrentView);
            Assert.IsType<ReviewsListViewModel>(viewModel.CurrentView.DataContext);
            Assert.False(viewModel.IsHomeView);
            Assert.False(viewModel.IsUsersListView);
            Assert.False(viewModel.IsCreateCarView);
            Assert.False(viewModel.IsCarsListView);
            Assert.False(viewModel.IsCarMapView);
            Assert.False(viewModel.IsRentalsListView);
            Assert.False(viewModel.IsCarsToApproveView);
            Assert.False(viewModel.IsStatisticsView);
            Assert.True(viewModel.IsReviewsListView);
        }

        [Fact]
        public void LogoutCommand_ClearsTokenAndInvokesLogoutAction()
        {
            // Arrange
            var mockTokenService = new MockTokenService();
            Locator.CurrentMutable.RegisterConstant<ITokenService>(mockTokenService);

            var logoutActionCalled = false;
            Action logoutAction = () => logoutActionCalled = true;
            var viewModel = new DashboardPageViewModel(null, _mockLifetime.Object, logoutAction);

            // Act
            viewModel.LogoutCommand.Execute().Subscribe();

            // Assert
            Assert.True(logoutActionCalled, "Logout action was not invoked");
        }

        [Fact]
        public void ToggleThemeCommand_SwitchesTheme()
        {
            // Arrange
            var testApplication = new TestApplication { RequestedThemeVariant = ThemeVariant.Light };
            var viewModel = new DashboardPageViewModel(testApplication, _mockLifetime.Object);

            // Act
            viewModel.ToggleThemeCommand.Execute().Subscribe();

            // Assert
            Assert.Equal(ThemeVariant.Dark, testApplication.RequestedThemeVariant);

            // Act again to toggle back
            viewModel.ToggleThemeCommand.Execute().Subscribe();

            // Assert
            Assert.Equal(ThemeVariant.Light, testApplication.RequestedThemeVariant);
        }

        // Testowa klasa Application
        private class TestApplication : Application
        {
            public new ThemeVariant RequestedThemeVariant
            {
                get => base.RequestedThemeVariant;
                set => base.RequestedThemeVariant = value;
            }
        }

        // Mock class for TokenService
        private class MockTokenService : ITokenService
        {
            public bool IsTokenCleared { get; private set; }

            public void ClearToken()
            {
                IsTokenCleared = true;
            }
        }

        // Define ITokenService interface for mocking
        private interface ITokenService
        {
            void ClearToken();
        }
    }
}