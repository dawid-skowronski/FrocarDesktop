using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class HomePageAdminTests
    {
        [Fact]
        public void Constructor_InitializesComponent()
        {
            // Arrange & Act
            var homePageAdmin = new HomePageAdmin();

            // Assert
            Assert.NotNull(homePageAdmin);
        }
    }
}