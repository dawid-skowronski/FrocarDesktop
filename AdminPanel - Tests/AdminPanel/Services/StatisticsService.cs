using AdminPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class StatisticsService
    {
        public static async Task<(bool IsSuccess, StatisticsDto Statistics, string Message)> GetStatistics()
        {
            try
            {
                var usersResult = await UserService.GetUsers();
                var carsResult = await CarService.GetCarListings();
                var rentalsResult = await RentalService.GetCarRentals();
                var reviewsResult = await ReviewService.GetReviews();

                if (!usersResult.IsSuccess || !rentalsResult.IsSuccess || !reviewsResult.IsSuccess)
                {
                    string errorMessage = usersResult.Message ?? rentalsResult.Message ?? reviewsResult.Message ?? "Błąd podczas pobierania danych statystyk";
                    return (false, null, errorMessage);
                }

                var users = usersResult.Users ?? new List<UserDto>();
                var cars = carsResult ?? new List<CarListing>();
                var rentals = rentalsResult.Rentals ?? new List<CarRentalDto>();
                var reviews = reviewsResult.Reviews ?? new List<ReviewDto>();

                var stats = new StatisticsDto
                {
                    TotalUsers = users.Count,
                    AdminUsers = users.Count(u => u.Role == "Admin"),
                    RegularUsers = users.Count(u => u.Role == "User"),
                    TotalCars = cars.Count,
                    AvailableCars = cars.Count(c => c.IsAvailable),
                    PendingApprovalCars = cars.Count(c => !c.IsApproved),
                    ActiveRentals = rentals.Count(r => r.RentalStatus == "Active"),
                    TotalRevenue = rentals.Sum(r => r.RentalPrice),
                    TotalReviews = reviews.Count
                };

                stats.RevenueLast30Days = rentals
                    .Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-30))
                    .Sum(r => r.RentalPrice);
                stats.RevenueLast60Days = rentals
                    .Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-60))
                    .Sum(r => r.RentalPrice);
                stats.RevenueLast90Days = rentals
                    .Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-90))
                    .Sum(r => r.RentalPrice);
                stats.RevenueLast365Days = rentals
                    .Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-365))
                    .Sum(r => r.RentalPrice);

                stats.TopSpenders = rentals
                    .GroupBy(r => r.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        TotalSpent = g.Sum(r => r.RentalPrice)
                    })
                    .OrderByDescending(g => g.TotalSpent)
                    .Take(3)
                    .Select((g, index) =>
                    {
                        var user = users.FirstOrDefault(u => u.Id == g.UserId);
                        return new TopSpender
                        {
                            Rank = index + 1,
                            Username = user?.Username ?? "Brak danych",
                            Email = user?.Email ?? "Brak danych",
                            TotalSpent = g.TotalSpent
                        };
                    })
                    .ToList();

                stats.TopProfitableCars = rentals
                    .GroupBy(r => r.CarListingId)
                    .Select(g => new
                    {
                        CarListingId = g.Key,
                        TotalRevenue = g.Sum(r => r.RentalPrice)
                    })
                    .OrderByDescending(g => g.TotalRevenue)
                    .Take(3)
                    .Select((g, index) =>
                    {
                        var car = cars.FirstOrDefault(c => c.Id == g.CarListingId);
                        var owner = users.FirstOrDefault(u => u.Id == car?.UserId);
                        return new TopProfitableCar
                        {
                            Rank = index + 1,
                            Brand = car?.Brand ?? "Brak danych",
                            OwnerUsername = owner?.Username ?? "Brak danych",
                            TotalRevenue = g.TotalRevenue
                        };
                    })
                    .ToList();

                stats.TopRatedCars = reviews
                    .GroupBy(r => r.CarRental.CarListingId)
                    .Select(g => new
                    {
                        CarListingId = g.Key,
                        AverageRating = g.Average(r => r.Rating)
                    })
                    .OrderByDescending(g => g.AverageRating)
                    .Take(3)
                    .Select((g, index) =>
                    {
                        var car = cars.FirstOrDefault(c => c.Id == g.CarListingId);
                        var owner = users.FirstOrDefault(u => u.Id == car?.UserId);
                        return new TopRatedCar
                        {
                            Rank = index + 1,
                            Brand = car?.Brand ?? "Brak danych",
                            OwnerUsername = owner?.Username ?? "Brak danych",
                            AverageRating = Math.Round(g.AverageRating, 2)
                        };
                    })
                    .ToList();

                var topSpender = stats.TopSpenders.FirstOrDefault();
                stats.TopSpenderUsername = topSpender?.Username ?? "Brak danych";
                stats.TopSpenderAmount = topSpender?.TotalSpent ?? 0m;

                var userWithMostCars = cars
                    .GroupBy(c => c.UserId)
                    .Select(g => new { UserId = g.Key, CarCount = g.Count() })
                    .OrderByDescending(g => g.CarCount)
                    .FirstOrDefault();
                if (userWithMostCars != null)
                {
                    var carOwner = users.FirstOrDefault(u => u.Id == userWithMostCars.UserId);
                    stats.UserWithMostCarsUsername = carOwner?.Username ?? "Brak danych";
                    stats.UserWithMostCarsCount = userWithMostCars.CarCount;
                }
                else
                {
                    stats.UserWithMostCarsUsername = "Brak danych";
                    stats.UserWithMostCarsCount = 0;
                }

                stats.AverageRentalPrice = cars.Any() ? Convert.ToDecimal(cars.Average(c => c.RentalPricePerDay)) : 0m;
                stats.CurrentlyRentedCars = rentals.Count(r => r.RentalStatus == "Active");
                stats.CompletedRentals = rentals.Count(r => r.RentalStatus == "Ended");
                stats.LongestRentalDuration = (int)(rentals.Any()
                    ? rentals.Max(r => (r.RentalEndDate - r.RentalStartDate).TotalDays)
                    : 0);
                stats.MostExpensiveRentalCost = rentals.Any() ? rentals.Max(r => r.RentalPrice) : 0m;
                stats.AverageRentalCost = rentals.Any() ? rentals.Average(r => r.RentalPrice) : 0m;
                stats.MostPopularCarBrand = cars
                    .GroupBy(c => c.Brand)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? "Brak danych";
                stats.AverageRentalDuration = rentals.Any()
                    ? rentals.Average(r => (r.RentalEndDate - r.RentalStartDate).TotalDays)
                    : 0;

                return (true, stats, "Pobrano statystyki pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }
    }
}