using AdminPanel.Services;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Avalonia.Controls;

namespace AdminPanel.Tests.ViewModels
{
    public class RegisterPageViewModelTests
    {
        private readonly Mock<Func<string, string, string, string, Task<(bool, string)>>> _registerMock;
        private readonly Mock<IWindowProvider> _windowProviderMock;
        private readonly Mock<INavigationService> _navigationServiceMock;

        public RegisterPageViewModelTests()
        {
            _registerMock = new Mock<Func<string, string, string, string, Task<(bool, string)>>>();
            _windowProviderMock = new Mock<IWindowProvider>();
            _navigationServiceMock = new Mock<INavigationService>();
            UserService.RegisterDelegate = _registerMock.Object;
        }

        [Fact]
        public async Task RegisterAsync_EmptyFields_SetsErrorMessage()
        {
            // Arrange
            var viewModel = new RegisterPageViewModel(_navigationServiceMock.Object)
            {
                Username = "",
                Email = "",
                Password = "",
                ConfirmPassword = ""
            };

            // Act
            await viewModel.RegisterAsync();

            // Assert
            Assert.Equal("Wszystkie pola są wymagane.", viewModel.ErrorMessage);
            _registerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_PasswordsDoNotMatch_SetsErrorMessage()
        {
            // Arrange
            var viewModel = new RegisterPageViewModel(_navigationServiceMock.Object)
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "different123"
            };

            // Act
            await viewModel.RegisterAsync();

            // Assert
            Assert.Equal("Hasła się nie zgadzają.", viewModel.ErrorMessage);
            _registerMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_SuccessfulRegistration_NavigatesToLoginPage()
        {
            // Arrange
            var viewModel = new RegisterPageViewModel(_navigationServiceMock.Object)
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };
            _registerMock.Setup(x => x("testuser", "test@example.com", "password123", "password123"))
                .ReturnsAsync((true, "Rejestracja zakończona sukcesem"));

            // Act
            await viewModel.RegisterAsync();

            // Assert
            Assert.Empty(viewModel.ErrorMessage);
            _registerMock.Verify(x => x("testuser", "test@example.com", "password123", "password123"), Times.Once);
            _navigationServiceMock.Verify(x => x.NavigateTo<LoginPage>(It.IsAny<LoginPageViewModel>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_FailedRegistration_SetsErrorMessage()
        {
            // Arrange
            var viewModel = new RegisterPageViewModel(_navigationServiceMock.Object)
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };
            _registerMock.Setup(x => x("testuser", "test@example.com", "password123", "password123"))
                .ReturnsAsync((false, "Użytkownik już istnieje"));

            // Act
            await viewModel.RegisterAsync();

            // Assert
            Assert.Equal("Użytkownik już istnieje", viewModel.ErrorMessage);
            _registerMock.Verify(x => x("testuser", "test@example.com", "password123", "password123"), Times.Once);
        }

        [Fact]
        public void GoBackToHome_NavigatesToHomePage()
        {
            // Arrange
            var viewModel = new RegisterPageViewModel(_navigationServiceMock.Object);

            // Act
            viewModel.GoBackToHome();

            // Assert
            _navigationServiceMock.Verify(x => x.NavigateTo<HomePageView>(It.IsAny<HomePageViewModel>()), Times.Once);
        }
    }
}