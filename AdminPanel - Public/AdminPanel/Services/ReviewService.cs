using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static class ReviewService
    {
        private const string BaseUrl = "https://projekt-tripify.hostingasp.pl/";
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

        public static async Task<(bool IsSuccess, List<ReviewDto> Reviews, string Message)> GetReviews()
        {
            try
            {
                var request = CreateAuthenticatedRequest("api/Admin/reviews", Method.Get);
                var response = await _client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    var reviews = JsonSerializer.Deserialize<List<ReviewDto>>(response.Content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (true, reviews ?? new List<ReviewDto>(), "Pobrano oceny pomyślnie");
                }
                return (false, null, HandleResponse(response, "").Message);
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteReview(int reviewId)
        {
            try
            {
                var request = CreateAuthenticatedRequest($"api/Admin/review/{reviewId}", Method.Delete);
                var response = await _client.ExecuteAsync(request);
                return HandleResponse(response, "Recenzja usunięta pomyślnie");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}