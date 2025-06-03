using AdminPanel.Services;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using Moq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public class HomePageViewModelTests
    {
        private readonly Mock<INavigationService> _navigationServiceMock;

        public HomePageViewModelTests()
        {
            _navigationServiceMock = new Mock<INavigationService>();
        }

        [Fact]
        public void Constructor_InitializesCommands_CommandsAreNotNull()
        {
            // Arrange & Act
            var viewModel = new HomePageViewModel(_navigationServiceMock.Object);

            // Assert
            Assert.NotNull(viewModel.LoginCommand);
            Assert.NotNull(viewModel.RegisterCommand);
        }

        [Fact]
        public async Task LoginCommand_Execute_NavigatesToLoginPage()
        {
            // Arrange
            var viewModel = new HomePageViewModel(_navigationServiceMock.Object);

            // Act
            await viewModel.LoginCommand.Execute().ToTask();

            // Assert
            _navigationServiceMock.Verify(x => x.NavigateTo<LoginPage>(It.IsAny<LoginPageViewModel>()), Times.Once());
        }

        [Fact]
        public async Task RegisterCommand_Execute_NavigatesToRegisterPage()
        {
            // Arrange
            var viewModel = new HomePageViewModel(_navigationServiceMock.Object);

            // Act
            await viewModel.RegisterCommand.Execute().ToTask();

            // Assert
            _navigationServiceMock.Verify(x => x.NavigateTo<RegisterPage>(It.IsAny<RegisterPageViewModel>()), Times.Once());
        }
    }
}