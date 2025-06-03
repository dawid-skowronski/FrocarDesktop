using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class CreateCarTests
    {
        [Fact]
        public void Constructor_SetsDataContextToCreateCarViewModel()
        {
            // Arrange & Act
            var createCar = new CreateCar();

            // Assert
            Assert.NotNull(createCar.DataContext);
            Assert.IsType<CreateCarViewModel>(createCar.DataContext);
        }
    }
}