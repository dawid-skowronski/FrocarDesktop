using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class CarService
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

        private static (bool IsSuccess, string Message) HandleResponse(RestResponse response)
        {
            if (response.IsSuccessful)
            {
                return (true, "Operacja zakończona sukcesem");
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

        public static async Task<(bool IsSuccess, string Message)> CreateCar(CarListing car)
        {
            try
            {
                var request = CreateAuthenticatedRequest("api/CarListings/create", Method.Post);
                car.UserId = TokenService.GetUserId();
                request.AddJsonBody(car);

                var response = await _client.ExecuteAsync(request);
                var result = HandleResponse(response);
                return (result.IsSuccess, result.IsSuccess ? "Pojazd dodany pomyślnie!" : result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<List<CarListing>> GetCarListings()
        {
            var request = CreateAuthenticatedRequest("api/Admin/listings", Method.Get);
            var response = await _client.ExecuteAsync<List<CarListing>>(request);

            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                return response.Data;
            }

            return new List<CarListing>();
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteCarListing(int carId)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/CarListings/{carId}", Method.Delete);
                var response = await _client.ExecuteAsync(request);
                var result = HandleResponse(response);
                return (result.IsSuccess, result.IsSuccess ? "Pojazd usunięty pomyślnie" : result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateCarListing(CarListing car)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/CarListings/{car.Id}", Method.Put);
                request.AddJsonBody(car);

                var response = await _client.ExecuteAsync(request);
                var result = HandleResponse(response);
                return (result.IsSuccess, result.IsSuccess ? "Pojazd zaktualizowany pomyślnie" : result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateCarAvailability(int carListingId, bool isAvailable)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/CarListings/{carListingId}/availability", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddBody(isAvailable.ToString().ToLower(), ContentType.Json);

                var response = await _client.ExecuteAsync(request);
                var result = HandleResponse(response);
                return (result.IsSuccess, result.IsSuccess ? "Dostępność samochodu zmieniona pomyślnie" : result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> ApproveCarListing(int carId)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/CarListings/{carId}/approve", Method.Put);
                request.AddHeader("accept", "*/*");

                var response = await _client.ExecuteAsync(request);
                var result = HandleResponse(response);

                if (result.IsSuccess && !string.IsNullOrEmpty(response.Content))
                {
                    var parsedMessage = TryParseErrorMessage(response.Content);
                    if (!string.IsNullOrEmpty(parsedMessage))
                    {
                        return (true, parsedMessage);
                    }
                }

                return (result.IsSuccess, result.IsSuccess ? "Pojazd zatwierdzony pomyślnie" : result.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}