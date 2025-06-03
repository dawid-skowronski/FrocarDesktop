using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class RegisterPageTests
    {
        [Fact]
        public void Constructor_SetsDataContextToRegisterPageViewModel()
        {
            // Arrange & Act
            var registerPage = new RegisterPage();

            // Assert
            Assert.NotNull(registerPage.DataContext);
            Assert.IsType<RegisterPageViewModel>(registerPage.DataContext);
        }
    }
}