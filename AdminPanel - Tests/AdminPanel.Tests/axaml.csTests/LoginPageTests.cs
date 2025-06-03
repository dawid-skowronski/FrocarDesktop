using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class LoginPageTests
    {
        [Fact]
        public void Constructor_InitializesComponent()
        {
            // Arrange & Act
            var loginPage = new LoginPage();

            // Assert
            Assert.NotNull(loginPage);
        }
    }
}