using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class UserService
    {
        private static readonly RestClient _client = new RestClient("https://projekt-tripify.hostingasp.pl/");

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

        public static async Task<(bool IsSuccess, string Message)> UpdateUser(
    int userId,
    string username,
    string email,
    string password,
    string role = null)
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
                    password,
                    role
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

        public static async Task<(bool IsSuccess, string Message)> RequestPasswordReset(string email)
        {
            try
            {
                var request = new RestRequest("api/Account/request-password-reset", Method.Post);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Content-Type", "application/json");
                request.AddStringBody($"\"{email}\"", ContentType.Json);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return (true, "Prośba o reset hasła została wysłana");
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

                return (false, response.Content ?? "Błąd podczas wysyłania prośby o reset hasła");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd podczas wysyłania prośby o reset hasła: {ex.Message}");
            }
        }
    }
}