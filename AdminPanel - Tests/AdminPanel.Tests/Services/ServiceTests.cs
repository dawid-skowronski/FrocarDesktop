using AdminPanel.Models;
using AdminPanel.Services;
using Moq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace AdminPanel.Tests.Services
{
    public class ServiceTests
    {
        private readonly Mock<IRestClient> _mockClient;
        private readonly string _validJwtToken;

        public ServiceTests()
        {
            _mockClient = new Mock<IRestClient>();
            TokenService.ClearToken();
            // Poprawny token JWT z claimem nameidentifier=1
            _validJwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWRlbnRpZmllciI6IjEiLCJleHAiOjI1MzQwMjMwMDgwMH0.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            // Wstrzykujemy mockowany RestClient do fabryki
            RestClientFactory.SetClient(_mockClient.Object);
        }

        private void SetupRequestVerification(RestRequest request)
        {
            if (!string.IsNullOrEmpty(TokenService.Token))
            {
                Assert.Contains(request.Parameters, p => p.Type == ParameterType.HttpHeader && p.Name == "Authorization" && p.Value.ToString() == $"Bearer {TokenService.Token}");
            }
        }

        // TokenService Tests
        [Fact]
        public void TokenService_SetToken_SetsToken()
        {
            // Arrange
            var token = _validJwtToken;

            // Act
            TokenService.SetToken(token);

            // Assert
            Assert.Equal(token, TokenService.Token);
        }

        [Fact]
        public void TokenService_ClearToken_SetsTokenToNull()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);

            // Act
            TokenService.ClearToken();

            // Assert
            Assert.Null(TokenService.Token);
        }

        [Fact]
        public void TokenService_GetUserId_ReturnsUserIdFromValidToken()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);

            // Act
            var userId = TokenService.GetUserId();

            // Assert
            Assert.Equal(1, userId);
        }

        [Fact]
        public void TokenService_GetUserId_ReturnsZeroForInvalidToken()
        {
            // Arrange
            TokenService.SetToken("invalid-token");

            // Act
            var userId = TokenService.GetUserId();

            // Assert
            Assert.Equal(0, userId);
        }

        [Fact]
        public void TokenService_GetUserId_ReturnsZeroForNullToken()
        {
            // Arrange
            TokenService.ClearToken();

            // Act
            var userId = TokenService.GetUserId();

            // Assert
            Assert.Equal(0, userId);
        }

        // NotificationApiService Tests
        [Fact]
        public async Task NotificationApiService_GetNotifications_Success_ReturnsNotifications()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var notifications = new List<NotificationDto> { new NotificationDto { NotificationId = 1, Message = "Test", IsRead = false } };
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(notifications),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, result, message) = await NotificationApiService.GetNotifications();

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test", result[0].Message);
            Assert.Equal("Pobrano powiadomienia", message);
        }

        [Fact]
        public async Task NotificationApiService_GetNotifications_Unauthorized_ReturnsError()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = JsonSerializer.Serialize(new { message = "Musisz być zalogowany." }),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, result, message) = await NotificationApiService.GetNotifications();

            // Assert
            Assert.False(isSuccess);
            Assert.Null(result);
            Assert.Equal("Musisz być zalogowany.", message);
        }

        [Fact]
        public async Task NotificationApiService_MarkNotificationAsRead_Success_ReturnsSuccess()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await NotificationApiService.MarkNotificationAsRead(1);

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Powiadomienie oznaczone jako przeczytane", message);
        }

        // NotificationService Tests
        [Fact]
        public async Task NotificationService_MarkAsRead_Success_ReturnsSuccess()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);
            var service = new NotificationService();

            // Act
            var (isSuccess, message) = await service.MarkAsRead(1);

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Powiadomienie oznaczone jako przeczytane", message);
        }

        [Fact]
        public async Task NotificationService_MarkAllAsRead_Success_MarksUnreadNotifications()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var notifications = new List<NotificationDto>
            {
                new NotificationDto { NotificationId = 1, IsRead = false },
                new NotificationDto { NotificationId = 2, IsRead = true }
            };
            var getResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(notifications),
                ResponseStatus = ResponseStatus.Completed
            };
            var patchResponse = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.SetupSequence(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .ReturnsAsync(getResponse)
                .ReturnsAsync(patchResponse);
            var service = new NotificationService();

            // Act
            var (isSuccess, message) = await service.MarkAllAsRead();

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Wszystkie powiadomienia oznaczone jako przeczytane", message);
            _mockClient.Verify(c => c.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("Notification/1")), default), Times.Once());
        }

        // UserService Tests
        [Fact]
        public async Task UserService_Login_Success_SetsToken()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(new { token = _validJwtToken }),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default)).ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await UserService.Login("user", "password123");

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Zalogowano pomyślnie", message);
            Assert.Equal(_validJwtToken, TokenService.Token);
        }

        [Fact]
        public async Task UserService_Login_Failure_ReturnsError()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = JsonSerializer.Serialize(new { message = "Nieprawidłowe dane." }),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default)).ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await UserService.Login("user", "wrong");

            // Assert
            Assert.False(isSuccess);
            Assert.Equal("Nieprawidłowe dane.", message);
        }

        [Fact]
        public async Task UserService_Register_Success_ReturnsSuccess()
        {
            // Arrange
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default)).ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await UserService.Register("user", "user@example.com", "password123", "password123");

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Rejestracja zakończona sukcesem", message);
        }

        [Fact]
        public async Task UserService_GetUsers_Success_ReturnsUsers()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var users = new List<UserDto> { new UserDto { Id = 1, Username = "user", Role = "User" } };
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(users),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, result, message) = await UserService.GetUsers();

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Pobrano użytkowników", message);
        }

        [Fact]
        public async Task UserService_DeleteUser_Success_ReturnsSuccess()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await UserService.DeleteUser(1);

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Użytkownik usunięty pomyślnie", message);
        }

        // CarService Tests
        [Fact]
        public async Task CarService_CreateCar_Success_ReturnsSuccess()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var car = new CarListing { Id = 1, Brand = "Toyota" };
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await CarService.CreateCar(car);

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Pojazd dodany pomyślnie!", message);
            Assert.Equal(1, car.UserId); // Weryfikacja ustawienia UserId
        }

        [Fact]
        public async Task CarService_GetCarListings_Success_ReturnsCars()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var cars = new List<CarListing> { new CarListing { Id = 1, Brand = "Toyota", UserId = 1 } };
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(cars),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var result = await CarService.GetCarListings();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Toyota", result[0].Brand);
        }

        [Fact]
        public async Task CarService_ApproveCarListing_Success_ReturnsSuccess()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(new { message = "Approved" }),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await CarService.ApproveCarListing(1);

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Approved", message);
        }

        // RentalService Tests
        [Fact]
        public async Task RentalService_GetCarRentals_Success_ReturnsRentals()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var rentals = new List<CarRentalDto> { new CarRentalDto { CarRentalId = 1, RentalStatus = "Active", UserId = 1 } };
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(rentals),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, result, message) = await RentalService.GetCarRentals();

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Pobrano wypożyczenia", message);
        }

        [Fact]
        public async Task RentalService_UpdateRentalStatus_Success_ReturnsSuccess()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, message) = await RentalService.UpdateRentalStatus(1, "Active");

            // Assert
            Assert.True(isSuccess);
            Assert.Equal("Status wypożyczenia zmieniony pomyślnie", message);
        }

        // ReviewService Tests
        [Fact]
        public async Task ReviewService_GetReviews_Success_ReturnsReviews()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var reviews = new List<ReviewDto> { new ReviewDto { ReviewId = 1, Rating = 5, CarRental = new CarRentalDto { CarListingId = 1 } } };
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonSerializer.Serialize(reviews),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, result, message) = await ReviewService.GetReviews();

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Pobrano oceny pomyślnie", message);
        }

        [Fact]
        public async Task ReviewService_GetReviews_Unauthorized_ReturnsError()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = JsonSerializer.Serialize(new { message = "Musisz być zalogowany." }),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, result, message) = await ReviewService.GetReviews();

            // Assert
            Assert.False(isSuccess);
            Assert.Null(result);
            Assert.Equal("Błąd autoryzacji: Sprawdź token", message);
        }

        // StatisticsService Tests
        [Fact]
        public async Task StatisticsService_GetStatistics_Success_ReturnsStats()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var users = new List<UserDto> { new UserDto { Id = 1, Role = "Admin", Username = "admin" }, new UserDto { Id = 2, Role = "User", Username = "user" } };
            var cars = new List<CarListing> { new CarListing { Id = 1, IsAvailable = true, IsApproved = false, UserId = 1, Brand = "Toyota", RentalPricePerDay = 100 } };
            var rentals = new List<CarRentalDto> { new CarRentalDto { CarRentalId = 1, RentalStatus = "Active", RentalPrice = 100m, RentalStartDate = DateTime.Now, RentalEndDate = DateTime.Now.AddDays(1), UserId = 2, CarListingId = 1 } };
            var reviews = new List<ReviewDto> { new ReviewDto { ReviewId = 1, Rating = 5, CarRental = new CarRentalDto { CarListingId = 1 } } };

            _mockClient.SetupSequence(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonSerializer.Serialize(users),
                    ResponseStatus = ResponseStatus.Completed
                })
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonSerializer.Serialize(cars),
                    ResponseStatus = ResponseStatus.Completed
                })
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonSerializer.Serialize(rentals),
                    ResponseStatus = ResponseStatus.Completed
                })
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonSerializer.Serialize(reviews),
                    ResponseStatus = ResponseStatus.Completed
                });

            // Act
            var (isSuccess, stats, message) = await StatisticsService.GetStatistics();

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(stats);
            Assert.Equal(2, stats.TotalUsers);
            Assert.Equal(1, stats.AdminUsers);
            Assert.Equal(1, stats.RegularUsers);
            Assert.Equal(1, stats.TotalCars);
            Assert.Equal(1, stats.AvailableCars);
            Assert.Equal(1, stats.PendingApprovalCars);
            Assert.Equal(1, stats.ActiveRentals);
            Assert.Equal(100m, stats.TotalRevenue);
            Assert.Equal(1, stats.TotalReviews);
            Assert.Equal("Pobrano statystyki pomyślnie", message);
        }

        [Fact]
        public async Task StatisticsService_GetStatistics_Unauthorized_ReturnsError()
        {
            // Arrange
            TokenService.SetToken(_validJwtToken);
            var response = new RestResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = JsonSerializer.Serialize(new { message = "Musisz być zalogowany." }),
                ResponseStatus = ResponseStatus.Completed
            };
            _mockClient.Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), default))
                .Callback<RestRequest, System.Threading.CancellationToken>((req, _) => SetupRequestVerification(req))
                .ReturnsAsync(response);

            // Act
            var (isSuccess, stats, message) = await StatisticsService.GetStatistics();

            // Assert
            Assert.False(isSuccess);
            Assert.Null(stats);
            Assert.Equal("Musisz być zalogowany.", message);
        }
    }
}