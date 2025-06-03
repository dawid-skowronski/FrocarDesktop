using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using Avalonia.Controls;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public class CarsListViewModelTests
    {

        [Fact]
        public async Task ResetFilters_ClearsAllFiltersAndRestoresFilteredCars()
        {
            // Arrange
            var cars = new List<CarListing>
            {
                new CarListing { Id = 1, UserId = 1, Brand = "Toyota" },
                new CarListing { Id = 2, UserId = 2, Brand = "Honda" }
            };
            CarService.GetCarListingsDelegate = () => Task.FromResult(cars);
            UserService.GetUserFromIdDelegate = (userId) => Task.FromResult((true, new UserDto { Id = userId, Username = $"User{userId}" }, "Success"));

            var viewModel = new CarsListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();

            // Apply some filters
            viewModel.BrandFilter = "Toyota";
            viewModel.FilterCarsCommand.Execute().Subscribe();

            // Act
            viewModel.ResetFiltersCommand.Execute().Subscribe();

            // Assert
            Assert.Empty(viewModel.BrandFilter);
            Assert.Empty(viewModel.IdFilter);
            Assert.Empty(viewModel.UserIdFilter);
            Assert.Empty(viewModel.MinEngineCapacity);
            Assert.Empty(viewModel.MaxEngineCapacity);
            Assert.Empty(viewModel.MinSeatsFilter);
            Assert.Empty(viewModel.MinPrice);
            Assert.Empty(viewModel.MaxPrice);
            Assert.Empty(viewModel.Address);
            Assert.Empty(viewModel.MaxRange);
            Assert.Null(viewModel.Latitude);
            Assert.Null(viewModel.Longitude);
            Assert.Equal("Wszystkie rodzaje paliwa", ((ComboBoxItem)viewModel.FuelTypeFilter).Content);
            Assert.Equal("Wszystkie typy nadwozia", ((ComboBoxItem)viewModel.CarTypeFilter).Content);
            Assert.Equal("Wszystkie", ((ComboBoxItem)viewModel.IsAvailableFilter).Content);
            Assert.Equal("Wszystkie", ((ComboBoxItem)viewModel.IsApprovedFilter).Content);
            Assert.Equal(viewModel.Cars.Count, viewModel.FilteredCars.Count);
        }
    }
}