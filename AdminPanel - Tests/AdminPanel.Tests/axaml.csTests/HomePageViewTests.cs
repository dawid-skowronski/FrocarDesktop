using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class HomePageViewTests
    {
        [Fact]
        public void Constructor_InitializesComponent()
        {
            // Arrange & Act
            var homePageView = new HomePageView();

            // Assert
            Assert.NotNull(homePageView);
        }
    }
}