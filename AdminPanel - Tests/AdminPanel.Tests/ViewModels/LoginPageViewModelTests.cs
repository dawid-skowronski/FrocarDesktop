using AdminPanel.Services;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels;

public class LoginPageViewModelTests
{
    private readonly Mock<Func<string, string, Task<(bool, string)>>> _loginMock;
    private readonly Mock<INavigationService> _navigationServiceMock;

    public LoginPageViewModelTests()
    {
        _loginMock = new Mock<Func<string, string, Task<(bool, string)>>>();
        _navigationServiceMock = new Mock<INavigationService>();
        UserService.LoginDelegate = _loginMock.Object;
    }

    [Fact]
    public async Task LoginAsync_EmptyUsername_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new LoginPageViewModel(_navigationServiceMock.Object)
        {
            Username = "",
            Password = "password123"
        };

        // Act
        await viewModel.LoginAsync();

        // Assert
        Assert.Equal("Wszystkie pola są wymagane.", viewModel.ErrorMessage);
        _loginMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new LoginPageViewModel(_navigationServiceMock.Object)
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        await viewModel.LoginAsync();

        // Assert
        Assert.Equal("Wszystkie pola są wymagane.", viewModel.ErrorMessage);
        _loginMock.Verify(x => x(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_NavigatesToDashboard()
    {
        // Arrange
        var viewModel = new LoginPageViewModel(_navigationServiceMock.Object)
        {
            Username = "testuser",
            Password = "password123"
        };
        _loginMock.Setup(x => x("testuser", "password123"))
            .ReturnsAsync((true, "Zalogowano pomyślnie"));

        // Act
        await viewModel.LoginAsync();

        // Assert
        Assert.Empty(viewModel.ErrorMessage);
        _loginMock.Verify(x => x("testuser", "password123"), Times.Once);
        _navigationServiceMock.Verify(x => x.NavigateTo<DashboardPage>(It.IsAny<DashboardPageViewModel>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_FailedLogin_SetsErrorMessage()
    {
        // Arrange
        var viewModel = new LoginPageViewModel(_navigationServiceMock.Object)
        {
            Username = "testuser",
            Password = "wrongpassword"
        };
        _loginMock.Setup(x => x("testuser", "wrongpassword"))
            .ReturnsAsync((false, "Błędne dane logowania"));

        // Act
        await viewModel.LoginAsync();

        // Assert
        Assert.Equal("Błędne dane logowania", viewModel.ErrorMessage);
        _loginMock.Verify(x => x("testuser", "wrongpassword"), Times.Once);
        _navigationServiceMock.Verify(x => x.NavigateTo<DashboardPage>(It.IsAny<DashboardPageViewModel>()), Times.Never);
    }

    [Fact]
    public void GoBackToHome_NavigatesToHomePage()
    {
        // Arrange
        var viewModel = new LoginPageViewModel(_navigationServiceMock.Object);

        // Act
        viewModel.GoBackToHome();

        // Assert
        _navigationServiceMock.Verify(x => x.NavigateTo<HomePageView>(It.IsAny<HomePageViewModel>()), Times.Once);
    }
}