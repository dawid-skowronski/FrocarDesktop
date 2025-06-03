using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class DashboardPageTests
    {
        [Fact]
        public void Constructor_InitializesComponent()
        {
            // Arrange & Act
            var dashboardPage = new DashboardPage();

            // Assert
            Assert.NotNull(dashboardPage);
        }
    }
}