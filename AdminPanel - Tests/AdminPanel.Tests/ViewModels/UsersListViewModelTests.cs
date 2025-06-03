using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public sealed class UsersListViewModelTests : IDisposable
    {
        private readonly MockUserService _mockUserService;
        private readonly MockTokenService _mockTokenService;
        private readonly Mock<IMessageBoxService> _mockMessageBoxService;
        private readonly Mock<IConfirmDialogService> _mockConfirmDialogService;

        public UsersListViewModelTests()
        {
            _mockUserService = new MockUserService();
            _mockTokenService = new MockTokenService();
            _mockMessageBoxService = new Mock<IMessageBoxService>();
            _mockConfirmDialogService = new Mock<IConfirmDialogService>();

            // Ustawienie mocków dla dialogów
            UsersListViewModel.ShowMessageBoxDelegate = _mockMessageBoxService.Object.ShowDialogAsync;
            UsersListViewModel.ShowConfirmDeleteDialogDelegate = _mockConfirmDialogService.Object.ShowDialogAsync;
        }

        public void Dispose()
        {
            // Przywracanie oryginalnych delegacji
            UserService.GetUsersDelegate = null;
            UserService.DeleteUserDelegate = null;
            TokenService.GetUserIdDelegate = null;
            UsersListViewModel.ShowMessageBoxDelegate = null;
            UsersListViewModel.ShowConfirmDeleteDialogDelegate = null;
        }

        [Fact]
        public async Task Constructor_InitializesAndFetchesUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "User1", Email = "user1@example.com", Role = "User" }
            };
            _mockUserService.SetGetUsersResult(true, users, "Pobrano użytkowników");
            _mockTokenService.SetUserId(1);

            // Act
            var viewModel = new UsersListViewModel();

            // Assert
            await Task.Delay(100); // Czekaj na wykonanie asynchronicznej komendy
            Assert.True(_mockUserService.GetUsersCalled);
            Assert.Single(viewModel.Users);
            Assert.True(viewModel.Users[0].IsCurrentUser);
            Assert.NotNull(viewModel.Users[0].EditCommand);
            Assert.NotNull(viewModel.Users[0].DeleteCommand);
        }

        [Fact]
        public async Task RefreshCommand_FetchesUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "User1", Email = "user1@example.com", Role = "User" },
                new UserDto { Id = 2, Username = "User2", Email = "user2@example.com", Role = "Admin" }
            };
            _mockUserService.SetGetUsersResult(true, users, "Pobrano użytkowników");
            _mockTokenService.SetUserId(1);
            var viewModel = new UsersListViewModel();

            // Act
            await viewModel.RefreshCommand.Execute().ToTask();

            // Assert
            Assert.Equal(2, viewModel.Users.Count);
            Assert.True(viewModel.Users[0].IsCurrentUser);
            Assert.False(viewModel.Users[1].IsCurrentUser);
        }

        [Fact]
        public async Task SearchCommand_FiltersUsersByQuery()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "John", Email = "john@example.com", Role = "User" },
                new UserDto { Id = 2, Username = "Jane", Email = "jane@example.com", Role = "Admin" }
            };
            _mockUserService.SetGetUsersResult(true, users, "Pobrano");
            var viewModel = new UsersListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();
            viewModel.SearchQuery = "John";

            // Act
            await viewModel.SearchCommand.Execute().ToTask(); // Czekaj na zakończenie komendy

            // Assert
            Assert.Single(viewModel.Users);
            Assert.Equal("John", viewModel.Users[0].Username);
        }

        [Fact]
        public async Task SearchCommand_EmptyQuery_ResetsSearch()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "John", Email = "john@example.com", Role = "User" },
                new UserDto { Id = 2, Username = "Jane", Email = "jane@example.com", Role = "Admin" }
            };
            _mockUserService.SetGetUsersResult(true, users, "Pobrano");
            var viewModel = new UsersListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();
            viewModel.SearchQuery = "";

            // Act
            await viewModel.SearchCommand.Execute().ToTask();

            // Assert
            Assert.Equal(2, viewModel.Users.Count);
            Assert.Contains(viewModel.Users, u => u.Username == "John");
            Assert.Contains(viewModel.Users, u => u.Username == "Jane");
        }

        [Fact]
        public async Task ResetSearchCommand_ClearsQueryAndRestoresUsers()
        {
            // Arrange
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "John", Email = "john@example.com", Role = "User" },
                new UserDto { Id = 2, Username = "Jane", Email = "jane@example.com", Role = "Admin" }
            };
            _mockUserService.SetGetUsersResult(true, users, "Pobrano");
            var viewModel = new UsersListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();
            viewModel.SearchQuery = "John";
            await viewModel.SearchCommand.Execute().ToTask();

            // Act
            await viewModel.ResetSearchCommand.Execute().ToTask();

            // Assert
            Assert.Empty(viewModel.SearchQuery);
            Assert.Equal(2, viewModel.Users.Count);
            Assert.Contains(viewModel.Users, u => u.Username == "John");
            Assert.Contains(viewModel.Users, u => u.Username == "Jane");
        }

        [Fact]
        public async Task DeleteCommand_SelfUser_ShowsError()
        {
            // Arrange
            var user = new UserDto { Id = 1, Username = "User1", Email = "user1@example.com", Role = "User" };
            _mockUserService.SetGetUsersResult(true, new List<UserDto> { user }, "Pobrano");
            _mockTokenService.SetUserId(1);
            var viewModel = new UsersListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();

            // Act
            await user.DeleteCommand.Execute(user).ToTask();

            // Assert
            _mockMessageBoxService.Verify(m => m.ShowDialogAsync("Błąd", "Nie możesz usunąć własnego konta!"), Times.Once());
            Assert.False(_mockUserService.DeleteUserCalled);
            Assert.Single(viewModel.Users);
        }

        [Fact]
        public async Task DeleteCommand_Confirmed_DeletesUser()
        {
            // Arrange
            var user = new UserDto { Id = 2, Username = "User2", Email = "user2@example.com", Role = "User" };
            _mockUserService.SetGetUsersResult(true, new List<UserDto> { user }, "Pobrano");
            _mockUserService.SetDeleteUserResult(true, "Użytkownik usunięty pomyślnie");
            _mockTokenService.SetUserId(1);
            _mockConfirmDialogService.Setup(d => d.ShowDialogAsync(2)).ReturnsAsync(true);
            var viewModel = new UsersListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();

            // Act
            await user.DeleteCommand.Execute(user).ToTask();

            // Assert
            _mockConfirmDialogService.Verify(d => d.ShowDialogAsync(2), Times.Once());
            Assert.True(_mockUserService.DeleteUserCalled);
            _mockMessageBoxService.Verify(m => m.ShowDialogAsync("Sukces", "Użytkownik został usunięty!"), Times.Once());
            Assert.Empty(viewModel.Users);
        }

        [Fact]
        public async Task DeleteCommand_NotConfirmed_DoesNotDelete()
        {
            // Arrange
            var user = new UserDto { Id = 2, Username = "User2", Email = "user2@example.com", Role = "User" };
            _mockUserService.SetGetUsersResult(true, new List<UserDto> { user }, "Pobrano");
            _mockTokenService.SetUserId(1);
            _mockConfirmDialogService.Setup(d => d.ShowDialogAsync(2)).ReturnsAsync(false);
            var viewModel = new UsersListViewModel();
            await viewModel.RefreshCommand.Execute().ToTask();

            // Act
            await user.DeleteCommand.Execute(user).ToTask();

            // Assert
            _mockConfirmDialogService.Verify(d => d.ShowDialogAsync(2), Times.Once());
            Assert.False(_mockUserService.DeleteUserCalled);
            Assert.Single(viewModel.Users);
        }
    }

    internal class MockUserService
    {
        private Func<Task<(bool, List<UserDto>, string)>> _getUsersFunc;
        private Func<int, Task<(bool, string)>> _deleteUserFunc;
        public bool GetUsersCalled { get; private set; }
        public bool DeleteUserCalled { get; private set; }

        public void SetGetUsersResult(bool isSuccess, List<UserDto> users, string message)
        {
            _getUsersFunc = () =>
            {
                GetUsersCalled = true;
                return Task.FromResult((isSuccess, users, message));
            };
            UserService.GetUsersDelegate = _getUsersFunc;
        }

        public void SetDeleteUserResult(bool isSuccess, string message)
        {
            _deleteUserFunc = (userId) =>
            {
                DeleteUserCalled = true;
                return Task.FromResult((isSuccess, message));
            };
            UserService.DeleteUserDelegate = _deleteUserFunc;
        }
    }

    internal class MockTokenService
    {
        private Func<int> _getUserIdFunc;

        public void SetUserId(int userId)
        {
            _getUserIdFunc = () => userId;
            TokenService.GetUserIdDelegate = _getUserIdFunc;
        }
    }

    public interface IMessageBoxService
    {
        Task ShowDialogAsync(string title, string message);
    }

    public interface IConfirmDialogService
    {
        Task<bool> ShowDialogAsync(int userId);
    }
}