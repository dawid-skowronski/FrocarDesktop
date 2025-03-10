﻿using AdminPanel.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace AdminPanel.Services
{
    public static class ApiService
    {
        private static readonly RestClient _client = new RestClient("https://localhost:5001/");

        public static async Task<(bool IsSuccess, string Message)> Login(string username, string password)
        {
            var request = new RestRequest("api/Account/login", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var json = JsonDocument.Parse(response.Content);
                var token = json.RootElement.GetProperty("token").GetString();
                TokenService.SetToken(token);
                return (true, "Zalogowano pomyślnie");
            }

            // Dodana obsługa komunikatów błędów z JSON
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

            var errorMessage = response.Content ?? "Błędne dane logowania";
            return (false, errorMessage);
        }


        public static async Task<(bool IsSuccess, string Message)> Register(string username, string email, string password, string confirmPassword)
        {
            var request = new RestRequest("/api/Account/register", Method.Post);
            request.AddJsonBody(new
            {
                username = username,
                email = email,
                password = password,
                confirmPassword = confirmPassword
            });

            var response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                return (true, "Rejestracja zakończona sukcesem");
            }

            // Dodana obsługa komunikatów błędów z JSON
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

            var errorMessage = response.Content ?? "Wystąpił błąd podczas rejestracji";
            return (false, errorMessage);
        }


        public static async Task<(bool IsSuccess, List<UserDto> Users, string Message)> GetUsers()
        {
            var request = new RestRequest("api/Account/users", Method.Get);
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var users = JsonSerializer.Deserialize<List<UserDto>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, users, "Pobrano użytkowników");
            }

            return (false, null, response.Content ?? "Błąd podczas pobierania użytkowników");
        }


        public static async Task<(bool IsSuccess, string Message)> CreateCar(CarListing car)
        {
            try
            {
                var request = new RestRequest("api/CarListings/create", Method.Post);
                request.AddHeader("Authorization", $"Bearer {TokenService.Token}");
                request.AddHeader("Content-Type", "application/json");

                // Pobierz userId z tokena i ustaw w obiekcie CarListing
                var userId = TokenService.GetUserId(); // Upewnij się, że masz taką metodę!
                car.UserId = userId;

                request.AddJsonBody(car);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    Console.WriteLine("Pojazd dodany pomyślnie!");
                    return (true, "Pojazd dodany pomyślnie!");
                }

                Console.WriteLine($"Błąd API: {response.Content}");
                return (false, response.Content ?? "Błąd podczas dodawania pojazdu");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wyjątek: {ex.Message}");
                return (false, ex.Message);
            }
        }


        public static async Task<List<CarListing>> GetCarListings()
        {
            var request = new RestRequest("api/CarListings/List", Method.Get);
            request.AddHeader("Authorization", $"Bearer {TokenService.Token}");

            var response = await _client.ExecuteAsync<List<CarListing>>(request);

            if (response.StatusCode == HttpStatusCode.OK && response.Data != null)
            {
                return response.Data;
            }

            return new List<CarListing>(); // Pusta lista w razie błędu
        }

    }
}
