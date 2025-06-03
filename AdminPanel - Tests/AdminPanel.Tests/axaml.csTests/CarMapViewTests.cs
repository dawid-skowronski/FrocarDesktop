using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class CarMapViewTests
    {
        [Fact]
        public void Constructor_SetsDataContextToCarMapViewModel()
        {
            // Arrange & Act
            var carMapView = new CarMapView();

            // Assert
            Assert.NotNull(carMapView.DataContext);
            Assert.IsType<CarMapViewModel>(carMapView.DataContext);
        }
    }
}