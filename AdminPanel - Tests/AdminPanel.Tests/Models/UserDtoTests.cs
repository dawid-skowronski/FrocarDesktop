using AdminPanel.Models;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class UserDtoTests
    {
        [Fact]
        public void UserDto_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var user = new UserDto
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = "Admin",
                IsCurrentUser = true
            };

            // Act & Assert
            Assert.Equal(1, user.Id);
            Assert.Equal("testuser", user.Username);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("Admin", user.Role);
            Assert.True(user.IsCurrentUser);
        }

        [Fact]
        public void UserDto_DefaultValues_AreNullOrDefault()
        {
            // Arrange
            var user = new UserDto();

            // Act & Assert
            Assert.Equal(0, user.Id);
            Assert.Null(user.Username);
            Assert.Null(user.Email);
            Assert.Null(user.Role);
            Assert.False(user.IsCurrentUser);
            Assert.Null(user.EditCommand);
            Assert.Null(user.DeleteCommand);
        }

        [Fact]
        public void EditCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var user = new UserDto { Id = 1, Username = "testuser" };
            user.EditCommand = ReactiveCommand.Create<UserDto, Unit>(u => Unit.Default);

            // Act
            var result = user.EditCommand.Execute(user).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(user.EditCommand);
            Assert.Equal(Unit.Default, result);
        }

        [Fact]
        public void DeleteCommand_InitializedCorrectly_ExecutesWithoutError()
        {
            // Arrange
            var user = new UserDto { Id = 1, Username = "testuser" };
            user.DeleteCommand = ReactiveCommand.Create<UserDto, Unit>(u => Unit.Default);

            // Act
            var result = user.DeleteCommand.Execute(user).GetAwaiter().GetResult();

            // Assert
            Assert.NotNull(user.DeleteCommand);
            Assert.Equal(Unit.Default, result);
        }
    }
}