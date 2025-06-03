using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class CarsListTests
    {
        [Fact]
        public void Constructor_SetsDataContextToCarsListViewModel()
        {
            // Arrange & Act
            var carsList = new CarsList();

            // Assert
            Assert.NotNull(carsList.DataContext);
            Assert.IsType<CarsListViewModel>(carsList.DataContext);
        }
    }
}