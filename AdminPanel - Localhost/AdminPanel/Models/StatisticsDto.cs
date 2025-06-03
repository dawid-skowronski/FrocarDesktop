using System.Collections.Generic;

namespace AdminPanel.Models
{
    public class StatisticsDto
    {
        public int TotalUsers { get; set; }
        public int AdminUsers { get; set; }
        public int RegularUsers { get; set; }
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int PendingApprovalCars { get; set; }
        public int ActiveRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueLast30Days { get; set; }
        public decimal RevenueLast60Days { get; set; }
        public decimal RevenueLast90Days { get; set; }
        public decimal RevenueLast365Days { get; set; }
        public string? MostPopularCarBrand { get; set; }
        public double AverageRentalDuration { get; set; }
        public string? TopSpenderUsername { get; set; }
        public decimal TopSpenderAmount { get; set; }
        public string? UserWithMostCarsUsername { get; set; }
        public int UserWithMostCarsCount { get; set; }
        public decimal AverageRentalPrice { get; set; }
        public int CurrentlyRentedCars { get; set; }
        public int CompletedRentals { get; set; }
        public int LongestRentalDuration { get; set; }
        public decimal AverageRentalCost { get; set; }
        public decimal MostExpensiveRentalCost { get; set; }
        public List<TopSpender> TopSpenders { get; set; } = new List<TopSpender>();
        public List<TopProfitableCar> TopProfitableCars { get; set; } = new List<TopProfitableCar>();
        public List<TopRatedCar> TopRatedCars { get; set; } = new List<TopRatedCar>();
        public int TotalReviews { get; set; }
    }

    public class TopSpender
    {
        public int Rank { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class TopProfitableCar
    {
        public int Rank { get; set; }
        public string? Brand { get; set; }
        public string? OwnerUsername { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class TopRatedCar
    {
        public int Rank { get; set; }
        public string? Brand { get; set; }
        public string? OwnerUsername { get; set; }
        public double AverageRating { get; set; }
    }
}