using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class RentalService
    {
        private const string BaseUrl = "https://localhost:5001/";
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
            return (false, response.Content ?? $"Błąd: {response.StatusCode}");
        }

        public static async Task<(bool IsSuccess, List<CarRentalDto> Rentals, string Message)> GetCarRentals()
        {
            try
            {
                var request = CreateAuthenticatedRequest("api/Admin/rentals", Method.Get);
                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var rentals = JsonSerializer.Deserialize<List<CarRentalDto>>(response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, rentals ?? new List<CarRentalDto>(), "Pobrano wypożyczenia");
                }
                return (false, null, HandleResponse(response, "").Message);
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> UpdateRentalStatus(int rentalId, string status)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/CarRental/{rentalId}/status", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddBody($"\"{status}\"", ContentType.Json);

                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Status wypożyczenia zmieniony pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteCarRental(int rentalId)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/CarRental/{rentalId}", Method.Delete);
                request.AddHeader("accept", "*/*");

                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Wypożyczenie usunięte pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}