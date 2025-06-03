using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using AdminPanel.Views;
using Moq;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class NotificationPopupTests
    {
        [Fact]
        public void Constructor_WithNotificationAndService_SetsDataContextAndProperties()
        {
            // Arrange
            var notification = new NotificationDto { NotificationId = 1, Message = "Test" };
            var mockService = new Mock<NotificationService>();

            // Act
            var popup = new NotificationPopup(notification, mockService.Object);

            // Assert
            Assert.NotNull(popup.DataContext);
            Assert.IsType<NotificationPopupViewModel>(popup.DataContext);
            Assert.True(popup.IsVisible);
            Assert.NotNull(popup.RenderTransform);
        }
    }
}