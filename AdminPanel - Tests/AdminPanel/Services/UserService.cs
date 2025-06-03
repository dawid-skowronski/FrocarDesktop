using AdminPanel.Models;
using AdminPanel.Tests;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static partial class UserService
    {
        private static readonly IRestClient _client = RestClientFactory.CreateClient("https://localhost:5001/");

        public static Func<Task<(bool, List<UserDto>, string)>>? GetUsersDelegate { get; set; }
        public static Func<int, Task<(bool, string)>>? DeleteUserDelegate { get; set; }
        public static Func<int, string, string, string, Task<(bool, string)>>? UpdateUserDelegate { get; set; }
        public static Func<int, Task<(bool IsSuccess, UserDto User, string Message)>>? GetUserFromIdDelegate { get; set; }
        public static Func<string, string, string, string, Task<(bool, string)>>? RegisterDelegate { get; set; }
        public static Func<string, string, Task<(bool, string)>>? LoginDelegate { get; set; }

        public static async Task<(bool IsSuccess, string Message)> Login(string username, string password)
        {
            if (LoginDelegate != null)
            {
                return await LoginDelegate(username, password);
            }

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
            if (RegisterDelegate != null)
            {
                return await RegisterDelegate(username, email, password, confirmPassword);
            }

            var request = new RestRequest("/api/Account/register", Method.Post);
            request.AddJsonBody(new
            {
                username,
                email,
                password,
                confirmPassword
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
            if (GetUsersDelegate != null)
            {
                return await GetUsersDelegate();
            }

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
            if (GetUserFromIdDelegate != null)
            {
                return await GetUserFromIdDelegate(userId);
            }

            try
            {
                var request = new RestRequest($"api/Admin/user/{userId}", Method.Get);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token ?? string.Empty}");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var user = JsonSerializer.Deserialize<UserDto>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    return (true, user, "Pobrano użytkownika");
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        var json = JsonDocument.Parse(response.Content);
                        if (json.RootElement.TryGetProperty("message", out var message))
                        {
                            return (false, null, message.GetString() ?? "Błąd podczas pobierania użytkownika");
                        }
                    }
                    catch (JsonException) { }
                }

                return (false, null, response.Content ?? "Błąd podczas pobierania użytkownika");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd podczas pobierania użytkownika: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteUser(int userId)
        {
            if (DeleteUserDelegate != null)
            {
                return await DeleteUserDelegate(userId);
            }

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
            if (UpdateUserDelegate != null)
            {
                return await UpdateUserDelegate(userId, username, email, password);
            }
            try
            {
                var request = new RestRequest($"api/Admin/user/{userId}", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token ?? string.Empty}");
                request.AddHeader("Content-Type", "application/json");

                var body = new
                {
                    username = username ?? string.Empty,
                    email = email ?? string.Empty,
                    password = password ?? string.Empty
                };
                request.AddJsonBody(body);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return (true, string.Empty); 
                }

                if (!string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        var json = JsonDocument.Parse(response.Content);
                        if (json.RootElement.TryGetProperty("message", out var message))
                        {
                            return (false, message.GetString() ?? "Błąd podczas aktualizacji użytkownika");
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