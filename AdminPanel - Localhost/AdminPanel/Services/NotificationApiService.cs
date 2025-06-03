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
        private const string BaseUrl = "https://localhost:5001/";
        private static readonly RestClient _client = new RestClient(BaseUrl);

        private static RestRequest CreateAuthenticatedRequest(string resource, Method method)
        {
            var request = new RestRequest(resource, method);
            request.AddHeader("accept", "*/*");
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
            return request;
        }

        private static (bool IsSuccess, string Message) HandleResponse(RestResponse response, string successMessage)
        {
            if (response.IsSuccessful)
            {
                return (true, successMessage);
            }
            return (false, response.Content ?? $"Błąd: {response.StatusCode}");
        }

        public static async Task<(bool IsSuccess, List<NotificationDto> Notifications, string Message)> GetNotifications()
        {
            try
            {
                var request = CreateAuthenticatedRequest("api/Account/Notification", Method.Get);
                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NotificationDto>();
                    return (true, notifications, "Pobrano powiadomienia");
                }
                return (false, null, HandleResponse(response, "").Message);
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
                var request = CreateAuthenticatedRequest($"api/Account/Notification/{notificationId}", Method.Patch);
                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Powiadomienie oznaczone jako przeczytane");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}