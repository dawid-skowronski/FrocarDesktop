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
                var cars = await CarService.GetCarListings(); 
                var rentalsResult = await RentalService.GetCarRentals();
                var reviewsResult = await ReviewService.GetReviews();

                if (!usersResult.IsSuccess || !rentalsResult.IsSuccess || !reviewsResult.IsSuccess)
                {
                    string errorMessage = usersResult.Message ?? rentalsResult.Message ?? reviewsResult.Message ?? "Błąd: Pobieranie danych statystyk nie powiodło się";
                    return (false, null, errorMessage);
                }

                var users = usersResult.Users ?? new List<UserDto>();
                var carListings = cars ?? new List<CarListing>(); 
                var rentals = rentalsResult.Rentals ?? new List<CarRentalDto>();
                var reviews = reviewsResult.Reviews ?? new List<ReviewDto>();

                var stats = new StatisticsDto
                {
                    TotalUsers = users.Count,
                    AdminUsers = users.Count(u => u.Role == "Admin"),
                    RegularUsers = users.Count(u => u.Role == "User"),
                    TotalCars = carListings.Count,
                    AvailableCars = carListings.Count(c => c.IsAvailable),
                    PendingApprovalCars = carListings.Count(c => !c.IsApproved),
                    ActiveRentals = rentals.Count(r => r.RentalStatus == "Active"),
                    TotalRevenue = rentals.Sum(r => r.RentalPrice),
                    TotalReviews = reviews.Count
                };

                CalculateRevenue(stats, rentals);
                stats.TopSpenders = CalculateTopSpenders(rentals, users);
                stats.TopProfitableCars = CalculateTopProfitableCars(rentals, carListings, users);
                stats.TopRatedCars = CalculateTopRatedCars(reviews, carListings, users);
                CalculateTopSpender(stats);
                CalculateUserWithMostCars(stats, carListings, users);
                CalculateAdditionalStats(stats, carListings, rentals);

                return (true, stats, "Pobrano statystyki pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        private static void CalculateRevenue(StatisticsDto stats, List<CarRentalDto> rentals)
        {
            stats.RevenueLast30Days = rentals.Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-30)).Sum(r => r.RentalPrice);
            stats.RevenueLast60Days = rentals.Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-60)).Sum(r => r.RentalPrice);
            stats.RevenueLast90Days = rentals.Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-90)).Sum(r => r.RentalPrice);
            stats.RevenueLast365Days = rentals.Where(r => r.RentalStartDate >= DateTime.Now.AddDays(-365)).Sum(r => r.RentalPrice);
        }

        private static List<TopSpender> CalculateTopSpenders(List<CarRentalDto> rentals, List<UserDto> users)
        {
            return rentals
                .GroupBy(r => r.UserId)
                .Select(g => new { UserId = g.Key, TotalSpent = g.Sum(r => r.RentalPrice) })
                .OrderByDescending(g => g.TotalSpent)
                .Take(3)
                .Select((g, index) => new TopSpender
                {
                    Rank = index + 1,
                    Username = users.FirstOrDefault(u => u.Id == g.UserId)?.Username ?? "Brak danych",
                    Email = users.FirstOrDefault(u => u.Id == g.UserId)?.Email ?? "Brak danych",
                    TotalSpent = g.TotalSpent
                })
                .ToList();
        }

        private static List<TopProfitableCar> CalculateTopProfitableCars(List<CarRentalDto> rentals, List<CarListing> cars, List<UserDto> users)
        {
            return rentals
                .GroupBy(r => r.CarListingId)
                .Select(g => new { CarListingId = g.Key, TotalRevenue = g.Sum(r => r.RentalPrice) })
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
        }

        private static List<TopRatedCar> CalculateTopRatedCars(List<ReviewDto> reviews, List<CarListing> cars, List<UserDto> users)
        {
            return reviews
                .GroupBy(r => r.CarRental.CarListingId)
                .Select(g => new { CarListingId = g.Key, AverageRating = g.Average(r => r.Rating) })
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
        }

        private static void CalculateTopSpender(StatisticsDto stats)
        {
            var topSpender = stats.TopSpenders.FirstOrDefault();
            stats.TopSpenderUsername = topSpender?.Username ?? "Brak danych";
            stats.TopSpenderAmount = topSpender?.TotalSpent ?? 0m;
        }

        private static void CalculateUserWithMostCars(StatisticsDto stats, List<CarListing> cars, List<UserDto> users)
        {
            var userWithMostCars = cars
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, CarCount = g.Count() })
                .OrderByDescending(g => g.CarCount)
                .FirstOrDefault();

            stats.UserWithMostCarsUsername = userWithMostCars != null
                ? users.FirstOrDefault(u => u.Id == userWithMostCars.UserId)?.Username ?? "Brak danych"
                : "Brak danych";
            stats.UserWithMostCarsCount = userWithMostCars?.CarCount ?? 0;
        }

        private static void CalculateAdditionalStats(StatisticsDto stats, List<CarListing> cars, List<CarRentalDto> rentals)
        {
            stats.AverageRentalPrice = cars.Any() ? Convert.ToDecimal(cars.Average(c => c.RentalPricePerDay)) : 0m;
            stats.CurrentlyRentedCars = rentals.Count(r => r.RentalStatus == "Active");
            stats.CompletedRentals = rentals.Count(r => r.RentalStatus == "Ended");
            stats.LongestRentalDuration = (int)(rentals.Any() ? rentals.Max(r => (r.RentalEndDate - r.RentalStartDate).TotalDays) : 0);
            stats.MostExpensiveRentalCost = rentals.Any() ? rentals.Max(r => r.RentalPrice) : 0m;
            stats.AverageRentalCost = rentals.Any() ? rentals.Average(r => r.RentalPrice) : 0m;
            stats.MostPopularCarBrand = cars.Any() ? cars.GroupBy(c => c.Brand).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "Brak danych" : "Brak danych";
            stats.AverageRentalDuration = rentals.Any() ? rentals.Average(r => (r.RentalEndDate - r.RentalStartDate).TotalDays) : 0;
        }
    }
}