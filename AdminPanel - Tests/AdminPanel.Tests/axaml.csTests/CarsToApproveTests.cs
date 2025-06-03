using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class CarsToApproveTests
    {
        [Fact]
        public void Constructor_SetsDataContextToCarsToApproveViewModel()
        {
            // Arrange & Act
            var carsToApprove = new CarsToApprove();

            // Assert
            Assert.NotNull(carsToApprove.DataContext);
            Assert.IsType<CarsToApproveViewModel>(carsToApprove.DataContext);
        }
    }
}