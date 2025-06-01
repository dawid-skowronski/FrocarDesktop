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
        private readonly RestClient _client;
        private readonly Timer _timer;
        private Action<List<NotificationDto>> _onNewNotifications;
        private List<NotificationDto> _lastNotifications = new List<NotificationDto>();

        public NotificationService()
        {
            _client = new RestClient("https://projekt-tripify.hostingasp.pl/");
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
                var request = new RestRequest("api/Account/Notification", Method.Get);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(
                        response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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

        public async Task<(bool IsSuccess, string Message)> MarkAsRead(int notificationId)
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

        public async Task<(bool IsSuccess, string Message)> MarkAllAsRead()
        {
            try
            {
                var request = new RestRequest("api/Account/Notification", Method.Get);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    return (false, response.Content ?? "Błąd podczas pobierania powiadomień");
                }

                var notifications = JsonSerializer.Deserialize<List<NotificationDto>>(
                    response.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();
                if (!unreadNotifications.Any())
                {
                    return (true, "Brak nieprzeczytanych powiadomień");
                }

                foreach (var notification in unreadNotifications)
                {
                    var markRequest = new RestRequest($"api/Account/Notification/{notification.NotificationId}", Method.Patch);
                    markRequest.AddHeader("accept", "*/*");
                    markRequest.AddHeader("Authorization", $"Bearer {TokenService.Token}");

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