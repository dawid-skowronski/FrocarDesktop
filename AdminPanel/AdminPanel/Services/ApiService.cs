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

            // Dodana obsługa komunikatów błędów z JSON
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

            // Dodana obsługa komunikatów błędów z JSON
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

                // Pobierz userId z tokena i ustaw w obiekcie CarListing
                var userId = TokenService.GetUserId(); // Upewnij się, że masz taką metodę!
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

            return new List<CarListing>(); // Pusta lista w razie błędu
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

        // In ApiService.cs, add this method
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


        // Metoda do usuwania użytkownika
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

        // Metoda do edycji użytkownika
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


    }
}
