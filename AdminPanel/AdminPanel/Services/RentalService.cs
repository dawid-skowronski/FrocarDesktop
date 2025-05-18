using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class RentalService
    {
        private static readonly RestClient _client = new RestClient("https://localhost:5001/");

        public static async Task<(bool IsSuccess, List<CarRentalDto> Rentals, string Message)> GetCarRentals()
        {
            try
            {
                var request = new RestRequest("api/Admin/rentals", Method.Get);
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var rentals = JsonSerializer.Deserialize<List<CarRentalDto>>(response.Content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (true, rentals, "Pobrano wypożyczenia");
                }

                return (false, null, response.Content ?? $"Błąd podczas pobierania wypożyczeń: {response.StatusCode}");
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
                var request = new RestRequest($"api/CarRental/{rentalId}/status", Method.Put);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");
                request.AddBody($"\"{status}\"", ContentType.Json);

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return (true, "Status wypożyczenia zmieniony pomyślnie");
                }

                return (false, response.Content ?? $"Błąd podczas zmiany statusu wypożyczenia: {response.StatusCode}");
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
                var request = new RestRequest($"api/CarRental/{rentalId}", Method.Delete);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return (true, "Wypożyczenie usunięte pomyślnie");
                }

                return (false, response.Content ?? $"Błąd podczas usuwania wypożyczenia: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}