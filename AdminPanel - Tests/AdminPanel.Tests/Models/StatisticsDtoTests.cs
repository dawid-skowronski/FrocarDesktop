using AdminPanel.Models;
using System.Collections.Generic;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class StatisticsDtoTests
    {
        [Fact]
        public void StatisticsDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var stats = new StatisticsDto
            {
                TotalUsers = 100,
                AdminUsers = 10,
                RegularUsers = 90,
                TotalCars = 50,
                AvailableCars = 30,
                PendingApprovalCars = 5,
                ActiveRentals = 15,
                TotalRevenue = 50000.00m,
                RevenueLast30Days = 10000.00m,
                RevenueLast60Days = 18000.00m,
                RevenueLast90Days = 25000.00m,
                RevenueLast365Days = 45000.00m,
                MostPopularCarBrand = "Toyota",
                AverageRentalDuration = 5.5,
                TopSpenderUsername = "bigspender",
                TopSpenderAmount = 7500.00m,
                UserWithMostCarsUsername = "carcollector",
                UserWithMostCarsCount = 8,
                AverageRentalPrice = 150.00m,
                CurrentlyRentedCars = 12,
                CompletedRentals = 200,
                LongestRentalDuration = 30,
                AverageRentalCost = 120.50m,
                MostExpensiveRentalCost = 500.00m,
                TotalReviews = 150,
                TopSpenders = new List<TopSpender>
                {
                    new TopSpender { Rank = 1, Username = "user1", Email = "user1@example.com", TotalSpent = 7500.00m }
                },
                TopProfitableCars = new List<TopProfitableCar>
                {
                    new TopProfitableCar { Rank = 1, Brand = "Toyota", OwnerUsername = "owner1", TotalRevenue = 12000.00m }
                },
                TopRatedCars = new List<TopRatedCar>
                {
                    new TopRatedCar { Rank = 1, Brand = "Honda", OwnerUsername = "owner2", AverageRating = 4.8 }
                }
            };

            // Act & Assert
            Assert.Equal(100, stats.TotalUsers);
            Assert.Equal(10, stats.AdminUsers);
            Assert.Equal(90, stats.RegularUsers);
            Assert.Equal(50, stats.TotalCars);
            Assert.Equal(30, stats.AvailableCars);
            Assert.Equal(5, stats.PendingApprovalCars);
            Assert.Equal(15, stats.ActiveRentals);
            Assert.Equal(50000.00m, stats.TotalRevenue);
            Assert.Equal(10000.00m, stats.RevenueLast30Days);
            Assert.Equal(18000.00m, stats.RevenueLast60Days);
            Assert.Equal(25000.00m, stats.RevenueLast90Days);
            Assert.Equal(45000.00m, stats.RevenueLast365Days);
            Assert.Equal("Toyota", stats.MostPopularCarBrand);
            Assert.Equal(5.5, stats.AverageRentalDuration);
            Assert.Equal("bigspender", stats.TopSpenderUsername);
            Assert.Equal(7500.00m, stats.TopSpenderAmount);
            Assert.Equal("carcollector", stats.UserWithMostCarsUsername);
            Assert.Equal(8, stats.UserWithMostCarsCount);
            Assert.Equal(150.00m, stats.AverageRentalPrice);
            Assert.Equal(12, stats.CurrentlyRentedCars);
            Assert.Equal(200, stats.CompletedRentals);
            Assert.Equal(30, stats.LongestRentalDuration);
            Assert.Equal(120.50m, stats.AverageRentalCost);
            Assert.Equal(500.00m, stats.MostExpensiveRentalCost);
            Assert.Equal(150, stats.TotalReviews);
            Assert.NotNull(stats.TopSpenders);
            Assert.Single(stats.TopSpenders);
            Assert.NotNull(stats.TopProfitableCars);
            Assert.Single(stats.TopProfitableCars);
            Assert.NotNull(stats.TopRatedCars);
            Assert.Single(stats.TopRatedCars);
        }

        [Fact]
        public void StatisticsDto_DefaultValues_AreNullOrDefault()
        {
            // Arrange
            var stats = new StatisticsDto();

            // Act & Assert
            Assert.Equal(0, stats.TotalUsers);
            Assert.Equal(0, stats.AdminUsers);
            Assert.Equal(0, stats.RegularUsers);
            Assert.Equal(0, stats.TotalCars);
            Assert.Equal(0, stats.AvailableCars);
            Assert.Equal(0, stats.PendingApprovalCars);
            Assert.Equal(0, stats.ActiveRentals);
            Assert.Equal(0m, stats.TotalRevenue);
            Assert.Equal(0m, stats.RevenueLast30Days);
            Assert.Equal(0m, stats.RevenueLast60Days);
            Assert.Equal(0m, stats.RevenueLast90Days);
            Assert.Equal(0m, stats.RevenueLast365Days);
            Assert.Null(stats.MostPopularCarBrand);
            Assert.Equal(0.0, stats.AverageRentalDuration);
            Assert.Null(stats.TopSpenderUsername);
            Assert.Equal(0m, stats.TopSpenderAmount);
            Assert.Null(stats.UserWithMostCarsUsername);
            Assert.Equal(0, stats.UserWithMostCarsCount);
            Assert.Equal(0m, stats.AverageRentalPrice);
            Assert.Equal(0, stats.CurrentlyRentedCars);
            Assert.Equal(0, stats.CompletedRentals);
            Assert.Equal(0, stats.LongestRentalDuration);
            Assert.Equal(0m, stats.AverageRentalCost);
            Assert.Equal(0m, stats.MostExpensiveRentalCost);
            Assert.Equal(0, stats.TotalReviews);
            Assert.Null(stats.TopSpenders);
            Assert.Null(stats.TopProfitableCars);
            Assert.Null(stats.TopRatedCars);
        }

        [Fact]
        public void TopSpender_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var topSpender = new TopSpender
            {
                Rank = 1,
                Username = "user1",
                Email = "user1@example.com",
                TotalSpent = 7500.00m
            };

            // Act & Assert
            Assert.Equal(1, topSpender.Rank);
            Assert.Equal("user1", topSpender.Username);
            Assert.Equal("user1@example.com", topSpender.Email);
            Assert.Equal(7500.00m, topSpender.TotalSpent);
        }

        [Fact]
        public void TopProfitableCar_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var topCar = new TopProfitableCar
            {
                Rank = 1,
                Brand = "Toyota",
                OwnerUsername = "owner1",
                TotalRevenue = 12000.00m
            };

            // Act & Assert
            Assert.Equal(1, topCar.Rank);
            Assert.Equal("Toyota", topCar.Brand);
            Assert.Equal("owner1", topCar.OwnerUsername);
            Assert.Equal(12000.00m, topCar.TotalRevenue);
        }

        [Fact]
        public void TopRatedCar_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var topRated = new TopRatedCar
            {
                Rank = 1,
                Brand = "Honda",
                OwnerUsername = "owner2",
                AverageRating = 4.8
            };

            // Act & Assert
            Assert.Equal(1, topRated.Rank);
            Assert.Equal("Honda", topRated.Brand);
            Assert.Equal("owner2", topRated.OwnerUsername);
            Assert.Equal(4.8, topRated.AverageRating);
        }
    }
}