using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace AdminPanel.Services
{
    public class NotificationService
    {
        private const string BaseUrl = "https://localhost:5001/";
        private readonly RestClient _client;
        private readonly Timer _timer;
        private Action<List<NotificationDto>> _onNewNotifications;
        private List<NotificationDto> _lastNotifications = new List<NotificationDto>();

        public NotificationService()
        {
            _client = new RestClient(BaseUrl);
            _timer = new Timer(1000);
            _timer.Elapsed += async (s, e) => await FetchNotifications();
            _timer.AutoReset = true;
        }

        public void Start(Action<List<NotificationDto>> onNewNotifications)
        {
            _onNewNotifications = onNewNotifications;
            _timer.Start();
            Task.Run(FetchNotifications);
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async Task FetchNotifications()
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

                    var newNotifications = notifications
                        .Where(n => !n.IsRead && !_lastNotifications.Any(ln => ln.NotificationId == n.NotificationId))
                        .ToList();

                    if (newNotifications.Any())
                    {
                        _onNewNotifications?.Invoke(newNotifications);
                    }

                    _lastNotifications = notifications;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas pobierania powiadomień: {ex.Message}");
            }
        }

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

        public async Task<(bool IsSuccess, string Message)> MarkAsRead(int notificationId)
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

        public async Task<(bool IsSuccess, string Message)> MarkAllAsRead()
        {
            try
            {
                var request = CreateAuthenticatedRequest("api/Account/Notification", Method.Get);
                var response = await _client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    return (false, response.Content ?? "Błąd: Pobieranie powiadomień nie powiodło się");
                }

                var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(
                    response.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NotificationDto>();

                var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();
                if (!unreadNotifications.Any())
                {
                    return (true, "Brak nieprzeczytanych powiadomień");
                }

                foreach (var notification in unreadNotifications)
                {
                    var markRequest = CreateAuthenticatedRequest($"api/Account/Notification/{notification.NotificationId}", Method.Patch);
                    var markResponse = await _client.ExecuteAsync(markRequest);
                    if (!markResponse.IsSuccessful)
                    {
                        return (false, $"Błąd podczas oznaczania powiadomienia ID: {notification.NotificationId}: {markResponse.Content}");
                    }
                }

                return (true, "Wszystkie powiadomienia oznaczone jako przeczytane");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}