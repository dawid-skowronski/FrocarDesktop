using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;

namespace AdminPanel.Services
{
    public static class ApiService
    {
        private static readonly RestClient _client = new RestClient("https://localhost:5001/");

        public static async Task<(bool IsSuccess, string Message)> Login(string username, string password)
        {
            var request = new RestRequest("api/Account/login", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var json = JsonDocument.Parse(response.Content);
                var token = json.RootElement.GetProperty("token").GetString();
                TokenService.SetToken(token);
                return (true, "Zalogowano pomyślnie");
            }

            if (!string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    var json = JsonDocument.Parse(response.Content);
                    if (json.RootElement.TryGetProperty("message", out var message))
                    {
                        return (false, message.GetString());
                    }
                }
                catch (JsonException) { }
            }

            var errorMessage = response.Content ?? "Błędne dane logowania";
            return (false, errorMessage);
        }

        public static async Task<(bool IsSuccess, string Message)> Register(string username, string email, string password, string confirmPassword)
        {
            var request = new RestRequest("/api/Account/register", Method.Post);
            request.AddJsonBody(new
            {
                username = username,
                email = email,
                password = password,
                confirmPassword = confirmPassword
            });

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                return (true, "Rejestracja zakończona sukcesem");
            }

            if (!string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    var json = JsonDocument.Parse(response.Content);
                    if (json.RootElement.TryGetProperty("message", out var message))
                    {
                        return (false, message.GetString());
                    }
                }
                catch (JsonException) { }
            }

            var errorMessage = response.Content ?? "Wystąpił błąd podczas rejestracji";
            return (false, errorMessage);
        }

        public static async Task<(bool IsSuccess, List<UserDto> Users, string Message)> GetUsers()
        {
            var request = new RestRequest("api/Admin/users", Method.Get);
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var users = JsonSerializer.Deserialize<List<UserDto>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, users, "Pobrano użytkowników");
            }

            return (false, null, response.Content ?? "Błąd podczas pobierania użytkowników");
        }

        public static async Task<(bool IsSuccess, UserDto User, string Message)> GetUserFromId(int userId)
        {
            var request = new RestRequest($"api/Admin/user/{userId}", Method.Get);
            request.AddHeader("accept", "*/*");
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var user = JsonSerializer.Deserialize<UserDto>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, user, "Pobrano użytkownika");
            }

            return (false, null, response.Content ?? "Błąd podczas pobierania użytkownika");
        }

        public static async Task<(bool IsSuccess, string Message)> CreateCar(CarListing car)
        {
            try
            {
                var request = new RestRequest("api/CarListings/create", Method.Post);
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");

                var userId = TokenService.GetUserId();
                car.UserId = userId;

                request.AddJsonBody(car);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    Console.WriteLine("Pojazd dodany pomyślnie!");
                    return (true, "Pojazd dodany pomyślnie!");
                }

                Console.WriteLine($"Błąd API: {response.Content}");
                return (false, response.Content ?? "Błąd podczas dodawania pojazdu");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wyjątek: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public static async Task<List<CarListing>> GetCarListings()
        {
            var request = new RestRequest("api/Admin/listings", Method.Get);
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

            var response = await _client.ExecuteAsync<List<CarListing>>(request);

            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                return response.Data;
            }

            return new List<CarListing>();
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteCarListing(int carId)
        {
            var request = new RestRequest($"api/CarListings/{carId}", Method.Delete);
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                return (true, "Pojazd usunięty pomyślnie");
            }

            return (false, response.Content ?? "Błąd podczas usuwania pojazdu");
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateCarListing(CarListing car)
        {
            try
            {
                var request = new RestRequest($"api/CarListings/{car.Id}", Method.Put);
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddJsonBody(car);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "Pojazd zaktualizowany pomyślnie");
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        var json = JsonDocument.Parse(response.Content);
                        if (json.RootElement.TryGetProperty("message", out var message))
                        {
                            return (false, message.GetString());
                        }
                    }
                    catch (JsonException) { }
                }

                return (false, response.Content ?? "Błąd podczas aktualizacji pojazdu");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteUser(int userId)
        {
            try
            {
                var request = new RestRequest($"api/Admin/user/{userId}", Method.Delete);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "Użytkownik usunięty pomyślnie");
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        var json = JsonDocument.Parse(response.Content);
                        if (json.RootElement.TryGetProperty("message", out var message))
                        {
                            return (false, message.GetString());
                        }
                    }
                    catch (JsonException) { }
                }

                return (false, response.Content ?? "Błąd podczas usuwania użytkownika");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd podczas usuwania użytkownika: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateUser(int userId, string username, string email, string password)
        {
            try
            {
                var request = new RestRequest($"api/Admin/user/{userId}", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    username,
                    email,
                    password
                };
                request.AddJsonBody(body);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return (true, "Użytkownik zaktualizowany pomyślnie");
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        var json = JsonDocument.Parse(response.Content);
                        if (json.RootElement.TryGetProperty("message", out var message))
                        {
                            return (false, message.GetString());
                        }
                    }
                    catch (JsonException) { }
                }

                return (false, response.Content ?? "Błąd podczas aktualizacji użytkownika");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd podczas aktualizacji użytkownika: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, List<CarRentalDto> Rentals, string Message)> GetCarRentals()
        {
            try
            {
                var request = new RestRequest("api/Admin/rentals", Method.Get);
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                Debug.WriteLine("Wysyłam żądanie GET /api/CarRental/list...");
                var response = await _client.ExecuteAsync(request);

                Debug.WriteLine($"Odpowiedź API: StatusCode={response.StatusCode}, Content={response.Content}");

                if (response.IsSuccessful)
                {
                    var rentals = JsonSerializer.Deserialize<List<CarRentalDto>>(response.Content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    Debug.WriteLine($"Zdeserializowano {rentals?.Count ?? 0} wypożyczeń.");
                    return (true, rentals, "Pobrano wypożyczenia");
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas pobierania wypożyczeń: {response.StatusCode}"
                    : response.Content;
                Debug.WriteLine($"Błąd API: {errorMessage}");
                return (false, null, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w GetCarRentals: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateRentalStatus(int rentalId, string status)
        {
            try
            {
                var request = new RestRequest($"api/CarRental/{rentalId}/status", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");
                request.AddBody($"\"{status}\"", ContentType.Json);

                Debug.WriteLine($"Wysyłam żądanie PUT /api/CarRental/{rentalId}/ RentalStatus={status}...");
                var response = await _client.ExecuteAsync(request);

                Debug.WriteLine($"Odpowiedź API: StatusCode={response.StatusCode}, Content={response.Content}");

                if (response.IsSuccessful)
                {
                    return (true, "Status wypożyczenia zmieniony pomyślnie");
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas zmiany statusu wypożyczenia: {response.StatusCode}"
                    : response.Content;
                Debug.WriteLine($"Błąd API: {errorMessage}");
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w UpdateRentalStatus: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateCarAvailability(int carListingId, bool isAvailable)
        {
            try
            {
                var request = new RestRequest($"api/CarListings/{carListingId}/availability", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");
                request.AddBody(isAvailable.ToString().ToLower(), ContentType.Json);

                Debug.WriteLine($"Wysyłam żądanie PUT /api/CarListings/{carListingId}/availability z isAvailable={isAvailable}...");
                var response = await _client.ExecuteAsync(request);

                Debug.WriteLine($"Odpowiedź API: StatusCode={response.StatusCode}, Content={response.Content}");

                if (response.IsSuccessful)
                {
                    return (true, "Dostępność samochodu zmieniona pomyślnie");
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas zmiany dostępności samochodu: {response.StatusCode}"
                    : response.Content;
                Debug.WriteLine($"Błąd API: {errorMessage}");
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w UpdateCarAvailability: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteCarRental(int rentalId)
        {
            try
            {
                var request = new RestRequest($"api/CarRental/{rentalId}", Method.Delete);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                Debug.WriteLine($"Wysyłam żądanie DELETE /api/CarRental/{rentalId}...");
                var response = await _client.ExecuteAsync(request);

                Debug.WriteLine($"Odpowiedź API: StatusCode={response.StatusCode}, Content={response.Content}");

                if (response.IsSuccessful)
                {
                    return (true, "Wypożyczenie usunięte pomyślnie");
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas usuwania wypożyczenia: {response.StatusCode}"
                    : response.Content;
                Debug.WriteLine($"Błąd API: {errorMessage}");
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w DeleteCarRental: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> ApproveCarListing(int carId)
        {
            try
            {
                var request = new RestRequest($"api/CarListings/{carId}/approve", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                Debug.WriteLine($"Wysyłam żądanie PUT /api/CarListings/{carId}/approve...");
                var response = await _client.ExecuteAsync(request);

                Debug.WriteLine($"Odpowiedź API: StatusCode={response.StatusCode}, Content={response.Content}");

                if (response.IsSuccessful)
                {
                    try
                    {
                        var json = JsonDocument.Parse(response.Content);
                        if (json.RootElement.TryGetProperty("message", out var message))
                        {
                            return (true, message.GetString());
                        }
                        return (true, "Pojazd zatwierdzony pomyślnie");
                    }
                    catch (JsonException)
                    {
                        return (true, "Pojazd zatwierdzony pomyślnie");
                    }
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas zatwierdzania pojazdu: {response.StatusCode}"
                    : response.Content;
                try
                {
                    var json = JsonDocument.Parse(response.Content);
                    if (json.RootElement.TryGetProperty("message", out var message))
                    {
                        errorMessage = message.GetString();
                    }
                }
                catch (JsonException) { }

                Debug.WriteLine($"Błąd API: {errorMessage}");
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w ApproveCarListing: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, $"Błąd: {ex.Message}");
            }
        }


        public static async Task<(bool IsSuccess, List<ReviewDto> Reviews, string Message)> GetReviews()
        {
            try
            {
                Debug.WriteLine($"GetReviews: Token={(string.IsNullOrEmpty(TokenService.Token) ? "BRAK TOKENU" : "Ustawiony")}");
                var request = new RestRequest("api/Admin/reviews", Method.Get);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                Debug.WriteLine("GetReviews: Wysyłam żądanie GET /api/Admin/reviews...");
                var response = await _client.ExecuteAsync(request);
                Debug.WriteLine($"GetReviews: Odpowiedź API: StatusCode={response.StatusCode}, Content={(response.Content?.Length > 1000 ? response.Content.Substring(0, 1000) + "..." : response.Content)}");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        var reviews = JsonSerializer.Deserialize<List<ReviewDto>>(response.Content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            //IgnoreUnknownProperties = true
                        });
                        Debug.WriteLine($"GetReviews: Zdeserializowano {reviews?.Count ?? 0} ocen.");
                        return (true, reviews ?? new List<ReviewDto>(), "Pobrano oceny pomyślnie");
                    }
                    catch (JsonException jsonEx)
                    {
                        Debug.WriteLine($"GetReviews: Błąd deserializacji: {jsonEx.Message}\nStackTrace: {jsonEx.StackTrace}");
                        return (false, null, $"Błąd deserializacji: {jsonEx.Message}");
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Debug.WriteLine("GetReviews: Błąd autoryzacji - nieprawidłowy lub brak tokenu.");
                    return (false, null, "Błąd autoryzacji: Sprawdź token");
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas pobierania ocen: {response.StatusCode}"
                    : response.Content;
                Debug.WriteLine($"GetReviews: Błąd API: {errorMessage}");
                return (false, null, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetReviews: Wyjątek: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteReview(int reviewId)
        {
            try
            {
                Debug.WriteLine($"DeleteReview: Usuwanie recenzji ID={reviewId}, Token={(string.IsNullOrEmpty(TokenService.Token) ? "BRAK TOKENU" : "Ustawiony")}");
                var request = new RestRequest($"api/Admin/review/{reviewId}", Method.Delete);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                Debug.WriteLine($"DeleteReview: Odpowiedź API: StatusCode={response.StatusCode}, Content={response.Content}");

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    return (true, "Recenzja usunięta pomyślnie");
                }

                string errorMessage = string.IsNullOrEmpty(response.Content)
                    ? $"Błąd podczas usuwania recenzji: {response.StatusCode}"
                    : response.Content;
                Debug.WriteLine($"DeleteReview: Błąd API: {errorMessage}");
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DeleteReview: Wyjątek: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, $"Błąd: {ex.Message}");
            }
        }





        public static async Task<(bool IsSuccess, StatisticsDto Statistics, string Message)> GetStatistics()
        {
            try
            {
                Debug.WriteLine("GetStatistics: Rozpoczynam pobieranie statystyk...");

                var usersResult = await GetUsers();
                var carsResult = await GetCarListings();
                var rentalsResult = await GetCarRentals();
                var reviewsResult = await GetReviews();

                Debug.WriteLine($"GetStatistics: UsersResult.IsSuccess: {usersResult.IsSuccess}, CarsResult: {carsResult?.Count ?? 0} samochodów, RentalsResult.IsSuccess: {rentalsResult.IsSuccess}, ReviewsResult.IsSuccess: {reviewsResult.IsSuccess}");
                Debug.WriteLine($"GetStatistics: UsersResult.Message: {usersResult.Message}, RentalsResult.Message: {rentalsResult.Message}, ReviewsResult.Message: {reviewsResult.Message}");

                if (!usersResult.IsSuccess || !rentalsResult.IsSuccess || !reviewsResult.IsSuccess)
                {
                    string errorMessage = usersResult.Message ?? rentalsResult.Message ?? reviewsResult.Message ?? "Błąd podczas pobierania danych statystyk";
                    Debug.WriteLine($"GetStatistics: Błąd - {errorMessage}");

                    var testStats = new StatisticsDto
                    {
                        TotalUsers = 10,
                        AdminUsers = 2,
                        RegularUsers = 8,
                        TotalCars = 5,
                        AvailableCars = 3,
                        PendingApprovalCars = 1,
                        ActiveRentals = 2,
                        TotalRevenue = 1500.00m,
                        RevenueLast30Days = 500.00m,
                        RevenueLast60Days = 800.00m,
                        RevenueLast90Days = 1200.00m,
                        RevenueLast365Days = 1500.00m,
                        MostPopularCarBrand = "Toyota",
                        AverageRentalDuration = 3.5,
                        TopSpenderUsername = "TestUser",
                        TopSpenderAmount = 1000.00m,
                        UserWithMostCarsUsername = "CarOwner",
                        UserWithMostCarsCount = 3,
                        AverageRentalPrice = 200.00m,
                        CurrentlyRentedCars = 2,
                        CompletedRentals = 5,
                        LongestRentalDuration = 10,
                        AverageRentalCost = 250.00m,
                        MostExpensiveRentalCost = 500.00m,
                        TotalReviews = 15, // Nowe pole w danych testowych
                        TopSpenders = new List<TopSpender>
                {
                    new TopSpender { Rank = 1, Username = "TestUser1", Email = "user1@test.com", TotalSpent = 1000.00m },
                    new TopSpender { Rank = 2, Username = "TestUser2", Email = "user2@test.com", TotalSpent = 800.00m },
                    new TopSpender { Rank = 3, Username = "TestUser3", Email = "user3@test.com", TotalSpent = 600.00m }
                },
                        TopProfitableCars = new List<TopProfitableCar>
                {
                    new TopProfitableCar { Rank = 1, Brand = "Toyota", OwnerUsername = "CarOwner1", TotalRevenue = 1200.00m },
                    new TopProfitableCar { Rank = 2, Brand = "Honda", OwnerUsername = "CarOwner2", TotalRevenue = 900.00m },
                    new TopProfitableCar { Rank = 3, Brand = "BMW", OwnerUsername = "CarOwner3", TotalRevenue = 700.00m }
                },
                        TopRatedCars = new List<TopRatedCar>
                {
                    new TopRatedCar { Rank = 1, Brand = "Toyota", OwnerUsername = "CarOwner1", AverageRating = 4.8 },
                    new TopRatedCar { Rank = 2, Brand = "Honda", OwnerUsername = "CarOwner2", AverageRating = 4.5 },
                    new TopRatedCar { Rank = 3, Brand = "BMW", OwnerUsername = "CarOwner3", AverageRating = 4.2 }
                }
                    };
                    return (true, testStats, "Użyto danych testowych z powodu błędu API");
                }

                var users = usersResult.Users ?? new List<UserDto>();
                var cars = carsResult ?? new List<CarListing>();
                var rentals = rentalsResult.Rentals ?? new List<CarRentalDto>();
                var reviews = reviewsResult.Reviews ?? new List<ReviewDto>();

                Debug.WriteLine($"GetStatistics: Pobrano dane - Użytkownicy: {users.Count}, Samochody: {cars.Count}, Wypożyczenia: {rentals.Count}, Oceny: {reviews.Count}");

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
                    TotalReviews = reviews.Count // Nowe pole
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

                Debug.WriteLine($"GetStatistics: Statystyki obliczone - TotalUsers: {stats.TotalUsers}, TotalCars: {stats.TotalCars}, TotalRevenue: {stats.TotalRevenue}, TotalReviews: {stats.TotalReviews}, TopRatedCars: {stats.TopRatedCars.Count}");

                return (true, stats, "Pobrano statystyki pomyślnie");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Wyjątek w GetStatistics: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, null, $"Błąd: {ex.Message}");
            }
        }
    }
}