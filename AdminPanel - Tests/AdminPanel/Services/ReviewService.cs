using AdminPanel.Models;
using AdminPanel.Tests;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdminPanel.Services
{
    public static partial class ReviewService
    {
        private static readonly IRestClient _client = RestClientFactory.CreateClient("https://localhost:5001/");

        public static Func<Task<(bool, List<ReviewDto>, string)>>? GetReviewsDelegate { get; set; }
        public static Func<int, Task<(bool, string)>>? DeleteReviewDelegate { get; set; }

        public static async Task<(bool IsSuccess, List<ReviewDto> Reviews, string Message)> GetReviews()
        {
            if (GetReviewsDelegate != null)
            {
                return await GetReviewsDelegate();
            }

            try
            {
                var request = new RestRequest("api/Admin/reviews", Method.Get);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    try
                    {
                        var reviews = JsonSerializer.Deserialize<List<ReviewDto>>(response.Content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        return (true, reviews ?? new List<ReviewDto>(), "Pobrano oceny pomyślnie");
                    }
                    catch (JsonException jsonEx)
                    {
                        return (false, null, $"Błąd deserializacji: {jsonEx.Message}");
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return (false, null, "Błąd autoryzacji: Sprawdź token");
                }

                return (false, null, response.Content ?? $"Błąd podczas pobierania ocen: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Błąd: {ex.Message}");
            }
        }

        public static async Task<(bool IsSuccess, string Message)> DeleteReview(int reviewId)
        {
            if (DeleteReviewDelegate != null)
            {
                return await DeleteReviewDelegate(reviewId);
            }

            try
            {
                var request = new RestRequest($"api/Admin/review/{reviewId}", Method.Delete);
                request.AddHeader("accept", "*/*");
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

                var response = await _client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
                {
                    return (true, "Recenzja usunięta pomyślnie");
                }

                return (false, response.Content ?? $"Błąd podczas usuwania recenzji: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return (false, $"Błąd: {ex.Message}");
            }
        }
    }
}