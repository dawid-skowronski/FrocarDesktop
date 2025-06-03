using AdminPanel.Models;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class ReviewDtoTests
    {
        [Fact]
        public void ReviewDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var createdAt = new DateTime(2025, 5, 31, 12, 59, 0);
            var user = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com" };
            var carRental = new CarRentalDto { CarRentalId = 100, Username = "testuser" };
            var review = new ReviewDto
            {
                ReviewId = 1,
                CarRentalId = 100,
                CarRental = carRental,
                UserId = 1,
                User = user,
                Rating = 5,
                Comment = "Great car and service!",
                CreatedAt = createdAt
            };

            // Act & Assert
            Assert.Equal(1, review.ReviewId);
            Assert.Equal(100, review.CarRentalId);
            Assert.Equal(carRental, review.CarRental);
            Assert.Equal(1, review.UserId);
            Assert.Equal(user, review.User);
            Assert.Equal(5, review.Rating);
            Assert.Equal("Great car and service!", review.Comment);
            Assert.Equal(createdAt, review.CreatedAt);
        }

        [Fact]
        public void ReviewDto_DefaultValues_AreNullOrDefault()
        {
            // Arrange
            var review = new ReviewDto();

            // Act & Assert
            Assert.Equal(0, review.ReviewId);
            Assert.Equal(0, review.CarRentalId);
            Assert.Null(review.CarRental);
            Assert.Equal(0, review.UserId);
            Assert.Null(review.User);
            Assert.Equal(0, review.Rating);
            Assert.Null(review.Comment);
            Assert.Equal(default(DateTime), review.CreatedAt);
            Assert.Null(review.DeleteCommand);
        }

        [Fact]
        public void DeleteCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var review = new ReviewDto { ReviewId = 1 };
            review.DeleteCommand = ReactiveCommand.Create<int, Unit>(id => Unit.Default);

            // Act
            var result = review.DeleteCommand.Execute(review.ReviewId).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(review.DeleteCommand);
            Assert.Equal(Unit.Default, result);
        }
    }
}