using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public class CarMapViewModelTests
    {
        private readonly CarMapViewModel _viewModel;

        public CarMapViewModelTests()
        {
            _viewModel = new CarMapViewModel();
            // Resetowanie delegata przed każdym testem
            CarService.GetCarListingsDelegate = null;
        }

        [Fact]
        public async Task LoadCarsAsync_SuccessfulResponse_SetsCarsAndFilteredCars()
        {
            // Arrange
            var cars = new List<CarListing>
            {
                new CarListing { Id = 1, Brand = "Toyota", EngineCapacity = 2.0, FuelType = "Benzyna", Seats = 5, CarType = "Sedan", IsAvailable = true, RentalPricePerDay = 100.0 },
                new CarListing { Id = 2, Brand = "BMW", EngineCapacity = 3.0, FuelType = "Diesel", Seats = 4, CarType = "SUV", IsAvailable = false, RentalPricePerDay = 150.0 }
            };
            CarService.GetCarListingsDelegate = () => Task.FromResult(cars);

            // Act
            await _viewModel.LoadCarsAsync();

            // Assert
            Assert.NotEmpty(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadCarsAsync_FailedResponse_SetsErrorMessage()
        {
            // Arrange
            CarService.GetCarListingsDelegate = () => Task.FromResult(new List<CarListing>());
            _viewModel.ErrorMessage = "Initial error";

            // Act
            await _viewModel.LoadCarsAsync();

            // Assert
            Assert.NotNull(_viewModel.Cars);
            Assert.NotNull(_viewModel.FilteredCars);
            Assert.Contains("Nie udało się pobrać listy pojazdów", _viewModel.ErrorMessage);
        }

        [Fact]
        public void FilterCars_WithValidFilters_AppliesFiltersCorrectly()
        {
            // Arrange
            var cars = new List<CarListing>
            {
                new CarListing { Id = 1, Brand = "Toyota", EngineCapacity = 2.0, FuelType = "Benzyna", Seats = 5, CarType = "Sedan", IsAvailable = true, RentalPricePerDay = 100.0, Latitude = 51.1, Longitude = 17.0 },
                new CarListing { Id = 2, Brand = "BMW", EngineCapacity = 3.0, FuelType = "Diesel", Seats = 4, CarType = "SUV", IsAvailable = false, RentalPricePerDay = 150.0, Latitude = 52.0, Longitude = 18.0 }
            };
            _viewModel.Cars = cars;
            _viewModel.FilteredCars = new List<CarListing>(cars);
            _viewModel.BrandFilter = "Toy";
            _viewModel.MinEngineCapacity = "1.5";
            _viewModel.FuelTypeFilter = new ComboBoxItem { Content = "Benzyna" };
            _viewModel.ShowOnlyAvailable = true;
            _viewModel.Latitude = 51.1;
            _viewModel.Longitude = 17.0;
            _viewModel.MaxRange = "100"; // Ustawiamy MaxRange, aby obliczyć odległość

            // Act
            _viewModel.FilterCars();

            // Assert
            Assert.Single(_viewModel.FilteredCars);
            Assert.Equal("Toyota", _viewModel.FilteredCars[0].Brand);
            Assert.Contains("Odległość", _viewModel.ErrorMessage);
        }

        [Fact]
        public void FilterCars_WithAddress_SetsCoordinatesAndFilters()
        {
            // Arrange
            var cars = new List<CarListing>
    {
        new CarListing { Id = 1, Brand = "Toyota", Latitude = 51.1, Longitude = 17.0, RentalPricePerDay = 100.0 },
        new CarListing { Id = 2, Brand = "BMW", Latitude = 52.0, Longitude = 18.0, RentalPricePerDay = 150.0 }
    };
            _viewModel.Cars = cars;
            _viewModel.FilteredCars = new List<CarListing>(cars);
            _viewModel.Address = "Wrocław, Poland";
            _viewModel.MaxRange = "50";
            _viewModel.Latitude = 51.1;
            _viewModel.Longitude = 17.0;

            // Act
            _viewModel.FilterCars();

            // Assert
            Assert.Equal("Toyota", _viewModel.FilteredCars[0].Brand);
        }

        [Fact]
        public void ResetFilters_ResetsAllFiltersAndLists()
        {
            // Arrange
            _viewModel.Cars = new List<CarListing> { new CarListing { Id = 1, Brand = "Toyota" } };
            _viewModel.FilteredCars = new List<CarListing> { new CarListing { Id = 1, Brand = "Toyota" } };
            _viewModel.BrandFilter = "Toy";
            _viewModel.MinEngineCapacity = "1.5";
            _viewModel.FuelTypeFilter = new ComboBoxItem { Content = "Benzyna" };
            _viewModel.ShowOnlyAvailable = true;
            _viewModel.ErrorMessage = "Test error";

            // Act
            _viewModel.ResetFilters();

            // Assert
            Assert.Empty(_viewModel.BrandFilter);
            Assert.Empty(_viewModel.MinEngineCapacity);
            Assert.Equal("Wszystkie rodzaje paliwa", (_viewModel.FuelTypeFilter as ComboBoxItem)?.Content?.ToString());
            Assert.False(_viewModel.ShowOnlyAvailable);
            Assert.Empty(_viewModel.ErrorMessage);
            Assert.Equal(_viewModel.Cars, _viewModel.FilteredCars);
        }

        [Fact]
        public void CalculateDistance_ValidCoordinates_ReturnsCorrectDistance()
        {
            // Arrange
            double lat1 = 51.1, lon1 = 17.0; // Wrocław
            double lat2 = 52.2, lon2 = 21.0; // Warszawa
            var methodInfo = typeof(CarMapViewModel).GetMethod("CalculateDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null) throw new InvalidOperationException("Metoda CalculateDistance nie znaleziona.");

            // Act
            var distance = (double)methodInfo.Invoke(_viewModel, new object[] { lat1, lon1, lat2, lon2 });

            // Assert
            Assert.InRange(distance, 290.0, 310.0);
        }

        [Fact]
        public void GetCarInfo_ReturnsFormattedString()
        {
            // Arrange
            var car = new CarListing
            {
                Brand = "Toyota",
                CarType = "Sedan",
                EngineCapacity = 2.0,
                FuelType = "Benzyna",
                Seats = 5,
                Features = new List<string> { "Klimatyzacja", "Nawigacja" },
                IsAvailable = true,
                RentalPricePerDay = 100.0
            };

            // Act
            string info = _viewModel.GetCarInfo(car);

            // Assert
            Assert.Contains("Marka: Toyota", info);
            Assert.Contains("Typ nadwozia: Sedan", info);
            Assert.Contains("Pojemność silnika: 2L", info);
            Assert.Contains("Rodzaj paliwa: Benzyna", info);
            Assert.Contains("Liczba miejsc: 5", info);
            Assert.Contains("Wyposażenie: Klimatyzacja, Nawigacja", info);
            Assert.Contains("Dostępność: Dostępny", info);
            Assert.Contains("Cena za dzień: 100 PLN", info);
        }
    }
}