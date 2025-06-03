using AdminPanel.Models;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class CarRentalDtoTests
    {
        [Fact]
        public void CarRentalDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var startDate = new DateTime(2025, 5, 31);
            var endDate = new DateTime(2025, 6, 5);
            var carListing = new CarListing { Id = 10, Brand = "Toyota" };
            var user = new UserDto { Id = 1, Username = "testuser" };
            var rental = new CarRentalDto
            {
                CarRentalId = 1,
                CarListingId = 10,
                CarListing = carListing,
                UserId = 1,
                User = user,
                Username = "testuser",
                RentalStartDate = startDate,
                RentalEndDate = endDate,
                RentalPrice = 250.00m,
                RentalStatus = "Active"
            };

            // Act & Assert
            Assert.Equal(1, rental.CarRentalId);
            Assert.Equal(10, rental.CarListingId);
            Assert.Equal(carListing, rental.CarListing);
            Assert.Equal(1, rental.UserId);
            Assert.Equal(user, rental.User);
            Assert.Equal("testuser", rental.Username);
            Assert.Equal(startDate, rental.RentalStartDate);
            Assert.Equal(endDate, rental.RentalEndDate);
            Assert.Equal(250.00m, rental.RentalPrice);
            Assert.Equal("Active", rental.RentalStatus);
        }

        [Fact]
        public void CarRentalDto_DefaultValues_AreNullOrDefault()
        {
            // Arrange
            var rental = new CarRentalDto();

            // Act & Assert
            Assert.Equal(0, rental.CarRentalId);
            Assert.Equal(0, rental.CarListingId);
            Assert.Null(rental.CarListing);
            Assert.Equal(0, rental.UserId);
            Assert.Null(rental.User);
            Assert.Null(rental.Username);
            Assert.Equal(default(DateTime), rental.RentalStartDate);
            Assert.Equal(default(DateTime), rental.RentalEndDate);
            Assert.Equal(0m, rental.RentalPrice);
            Assert.Null(rental.RentalStatus);
            Assert.Null(rental.CancelCommand);
            Assert.Null(rental.ResumeCommand);
            Assert.Null(rental.DeleteCommand);
        }

        [Fact]
        public void CancelCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var rental = new CarRentalDto { CarRentalId = 1 };
            rental.CancelCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = rental.CancelCommand.Execute(rental.CarRentalId).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(rental.CancelCommand);
            Assert.Equal(Unit.Default, result);
        }

        [Fact]
        public void ResumeCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var rental = new CarRentalDto { CarRentalId = 1 };
            rental.ResumeCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = rental.ResumeCommand.Execute(rental.CarRentalId).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(rental.ResumeCommand);
            Assert.Equal(Unit.Default, result);
        }

        [Fact]
        public void DeleteCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var rental = new CarRentalDto { CarRentalId = 1 };
            rental.DeleteCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = rental.DeleteCommand.Execute(rental.CarRentalId).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(rental.DeleteCommand);
            Assert.Equal(Unit.Default, result);
        }
    }
}