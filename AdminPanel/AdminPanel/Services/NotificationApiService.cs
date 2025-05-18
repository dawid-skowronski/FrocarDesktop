using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class NotificationApiService
    {
        private static readonly RestClient _client = new RestClient("https://localhost:5001/");

        public static async Task<(bool IsSuccess, List<NotificationDto> Notifications, string Message)> GetNotifications()
        {
            try
            {
                var request = new RestRequest("api/Account/Notification", Method.Get);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, notifications, "Pobrano powiadomienia");
                }

                return (false, null, response.Content ?? "Błąd podczas pobierania powiadomień");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var request = new RestRequest($"api/Account/Notification/{notificationId}", Method.Patch);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return (true, "Powiadomienie oznaczone jako przeczytane");
                }

                return (false, response.Content ?? "Błąd podczas oznaczania powiadomienia");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}