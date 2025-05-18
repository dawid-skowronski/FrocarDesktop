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
        private static readonly RestClient _client = new RestClient("https://localhost:5001/");

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
                    return (true, "Pojazd dodany pomyślnie!");
                }

                return (false, response.Content ?? "Błąd podczas dodawania pojazdu");
            }
            catch (Exception ex)
            {
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

        public static async Task<(bool IsSuccess, string Message)> UpdateCarAvailability(int carListingId, bool isAvailable)
        {
            try
            {
                var request = new RestRequest($"api/CarListings/{carListingId}/availability", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");
                request.AddBody(isAvailable.ToString().ToLower(), ContentType.Json);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return (true, "Dostępność samochodu zmieniona pomyślnie");
                }

                return (false, response.Content ?? $"Błąd podczas zmiany dostępności samochodu: {response.StatusCode}");
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
                var request = new RestRequest($"api/CarListings/{carId}/approve", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
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

                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}