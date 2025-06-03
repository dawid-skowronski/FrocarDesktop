using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class RentalsListTests
    {
        [Fact]
        public void Constructor_SetsDataContextToRentalsListViewModel()
        {
            // Arrange & Act
            var rentalsList = new RentalsList();

            // Assert
            Assert.NotNull(rentalsList.DataContext);
            Assert.IsType<RentalsListViewModel>(rentalsList.DataContext);
        }
    }
}