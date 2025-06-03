using AdminPanel.Models;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class NotificationDtoTests
    {
        [Fact]
        public void NotificationDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var user = new UserDto { Id = 1, Username = "testuser", Email = "test@example.com" };
            var notification = new NotificationDto
            {
                NotificationId = 1,
                UserId = 1,
                User = user,
                Message = "New car rental request",
                CreatedAt = "2025-05-31 12:59:00",
                IsRead = false
            };

            // Act & Assert
            Assert.Equal(1, notification.NotificationId);
            Assert.Equal(1, notification.UserId);
            Assert.Equal(user, notification.User);
            Assert.Equal("New car rental request", notification.Message);
            Assert.Equal("2025-05-31 12:59:00", notification.CreatedAt);
            Assert.False(notification.IsRead);
        }

        [Fact]
        public void NotificationDto_DefaultValues_AreNullOrDefault()
        {
            // Arrange
            var notification = new NotificationDto();

            // Act & Assert
            Assert.Equal(0, notification.NotificationId);
            Assert.Equal(0, notification.UserId);
            Assert.Null(notification.User);
            Assert.Null(notification.Message);
            Assert.Null(notification.CreatedAt);
            Assert.False(notification.IsRead);
        }
    }
}