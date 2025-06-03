using AdminPanel.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class CarListingTests
    {
        [Fact]
        public void CarListing_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var features = new List<string> { "AC", "GPS", "Bluetooth" };
            var car = new CarListing
            {
                Id = 1,
                Brand = "Toyota",
                EngineCapacity = 2.0,
                FuelType = "Petrol",
                Seats = 5,
                CarType = "Sedan",
                Features = features,
                Latitude = 52.5200,
                Longitude = 13.4050,
                UserId = 1,
                Username = "testuser",
                IsAvailable = true,
                IsApproved = false,
                RentalPricePerDay = 50.00,
                AverageRating = 4.5
            };

            // Act & Assert
            Assert.Equal(1, car.Id);
            Assert.Equal("Toyota", car.Brand);
            Assert.Equal(2.0, car.EngineCapacity);
            Assert.Equal("Petrol", car.FuelType);
            Assert.Equal(5, car.Seats);
            Assert.Equal("Sedan", car.CarType);
            Assert.Equal(features, car.Features);
            Assert.Equal(52.5200, car.Latitude);
            Assert.Equal(13.4050, car.Longitude);
            Assert.Equal(1, car.UserId);
            Assert.Equal("testuser", car.Username);
            Assert.True(car.IsAvailable);
            Assert.False(car.IsApproved);
            Assert.Equal(50.00, car.RentalPricePerDay);
            Assert.Equal(4.5, car.AverageRating);
            Assert.Equal("AC, GPS, Bluetooth", car.FeaturesAsString);
            Assert.Equal("52,520000, \n13,405000", car.LocationString);
        }

        [Fact]
        public void CarListing_DefaultValues_AreNullOrDefault()
        {
            // Arrange
            var car = new CarListing();

            // Act & Assert
            Assert.Equal(0, car.Id);
            Assert.Null(car.Brand);
            Assert.Equal(0.0, car.EngineCapacity);
            Assert.Null(car.FuelType);
            Assert.Equal(0, car.Seats);
            Assert.Null(car.CarType);
            Assert.Null(car.Features);
            Assert.Equal(0.0, car.Latitude);
            Assert.Equal(0.0, car.Longitude);
            Assert.Equal(0, car.UserId);
            Assert.Null(car.Username);
            Assert.False(car.IsAvailable);
            Assert.False(car.IsApproved);
            Assert.Equal(0.0, car.RentalPricePerDay);
            Assert.Null(car.AverageRating);
            Assert.Equal("", car.FeaturesAsString);
            Assert.Equal("0,000000, \n0,000000", car.LocationString);
            Assert.Null(car.DeleteCommand);
            Assert.Null(car.EditCommand);
            Assert.Null(car.ApproveCommand);
        }

        [Fact]
        public void DeleteCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var car = new CarListing { Id = 1 };
            car.DeleteCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = car.DeleteCommand.Execute(car.Id).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(car.DeleteCommand);
            Assert.Equal(Unit.Default, result);
        }

        [Fact]
        public void EditCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var car = new CarListing { Id = 1 };
            car.EditCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = car.EditCommand.Execute(car.Id).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(car.EditCommand);
            Assert.Equal(Unit.Default, result);
        }

        [Fact]
        public void ApproveCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var car = new CarListing { Id = 1 };
            car.ApproveCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = car.ApproveCommand.Execute(car.Id).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(car.ApproveCommand);
            Assert.Equal(Unit.Default, result);
        }
    }
}