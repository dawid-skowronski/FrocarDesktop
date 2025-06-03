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
        private const string BaseUrl = "https://projekt-tripify.hostingasp.pl/";
        private static readonly RestClient _client = new RestClient(BaseUrl);

        private static RestRequest CreateAuthenticatedRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
            request.AddHeader("Content-Type", "application/json");
            return request;
        }

        private static (bool IsSuccess, string Message) HandleResponse(RestResponse response, string successMessage)
        {
            if (response.IsSuccessful)
            {
                return (true, successMessage);
            }

            string errorMessage = TryParseErrorMessage(response.Content) ??
                $"Błąd: {response.StatusCode}";
            return (false, errorMessage);
        }

        private static string? TryParseErrorMessage(string? content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            try
            {
                using var json = JsonDocument.Parse(content);
                if (json.RootElement.TryGetProperty("message", out var message))
                {
                    return message.GetString();
                }
            }
            catch (JsonException)
            {
            }
            return content;
        }

        public static async Task<(bool IsSuccess, string Message)> Login(string username, string password)
        {
            try
            {
                var request = new RestRequest("api/Account/login", Method.Post);
                request.AddJsonBody(new { username, password });

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    using var json = JsonDocument.Parse(response.Content);
                    var token = json.RootElement.GetProperty("token").GetString();
                    TokenService.SetToken(token);
                    return (true, "Zalogowano pomyślnie");
                }
                return HandleResponse(response, "Zalogowano pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> Register(string username, string email, string password, string confirmPassword)
        {
            try
            {
                var request = new RestRequest("api/Account/register", Method.Post);
                request.AddJsonBody(new { username, email, password, confirmPassword });

                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Rejestracja zakończona sukcesem");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, List<UserDto> Users, string Message)> GetUsers()
        {
            try
            {
                var request = CreateAuthenticatedRequest("api/Admin/users", Method.Get);
                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var users = JsonSerializer.Deserialize<List<UserDto>>(response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, users ?? new List<UserDto>(), "Pobrano użytkowników");
                }
                return (false, null, TryParseErrorMessage(response.Content) ?? "Błąd: Pobieranie użytkowników nie powiodło się");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, UserDto User, string Message)> GetUserFromId(int userId)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/Admin/user/{userId}", Method.Get);
                request.AddHeader("accept", "*/*");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var user = JsonSerializer.Deserialize<UserDto>(response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, user, "Pobrano użytkownika");
                }
                return (false, null, TryParseErrorMessage(response.Content) ?? "Błąd: Pobieranie użytkownika nie powiodło się");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteUser(int userId)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/Admin/user/{userId}", Method.Delete);
                request.AddHeader("accept", "*/*");

                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Użytkownik usunięty pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateUser(int userId, string username, string email, string password, string role = null)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/Admin/user/{userId}", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddJsonBody(new { username, email, password, role });

                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Użytkownik zaktualizowany pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
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
                return HandleResponse(response, "Prośba o reset hasła została wysłana");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}