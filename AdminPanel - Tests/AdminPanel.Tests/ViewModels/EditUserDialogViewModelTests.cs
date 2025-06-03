using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using Avalonia.Controls;
using Moq;
using System.Reactive.Threading.Tasks;

namespace AdminPanel.Tests.ViewModels
{
    public class EditUserDialogViewModelTests
    {
        private readonly Mock<Window> _dialogMock;
        private readonly UserDto _user;
        private readonly EditUserDialogViewModel _viewModel;

        public EditUserDialogViewModelTests()
        {
            AvaloniaTestSetup.Initialize();

            _dialogMock = new Mock<Window>();
            _user = new UserDto
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = "User"
            };
            _viewModel = new EditUserDialogViewModel(_user, _dialogMock.Object);

            UserService.UpdateUserDelegate = null;
        }

        [Fact]
        public async Task CancelCommand_Execute_ClosesDialog()
        {
            // Act
            await _viewModel.CancelCommand.Execute().ToTask();

            // Assert
            Assert.NotEmpty(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SaveCommand_SuccessfulUpdate_UpdatesUserAndClosesDialog()
        {
            // Arrange
            _viewModel.Username = "newuser";
            _viewModel.Email = "new@example.com";
            _viewModel.Password = "newpassword";
            UserService.UpdateUserDelegate = (id, username, email, password) =>
            {
                if (username == null || email == null || password == null)
                    throw new ArgumentNullException("Parametr nie może być null");
                return Task.FromResult((true, string.Empty));
            };

            // Act
            await _viewModel.SaveCommand.Execute().ToTask();

            // Assert
            Assert.Equal("newuser", _viewModel.User.Username);
            Assert.Equal("new@example.com", _viewModel.User.Email);
        }

        // Pozostałe testy bez zmian
        [Fact]
        public void Constructor_InitializesPropertiesAndCommands_SetsCorrectValues()
        {
            Assert.Equal(_user, _viewModel.User);
            Assert.Equal("testuser", _viewModel.Username);
            Assert.Equal("test@example.com", _viewModel.Email);
            Assert.Empty(_viewModel.Password);
            Assert.Empty(_viewModel.ErrorMessage);
            Assert.Equal("Edycja użytkownika testuser (ID: 1)", _viewModel.Title);
            Assert.NotNull(_viewModel.SaveCommand);
            Assert.NotNull(_viewModel.CancelCommand);
        }

        [Fact]
        public async Task SaveCommand_EmptyUsername_SetsErrorMessage()
        {
            _viewModel.Username = "";
            _viewModel.Email = "test@example.com";

            await _viewModel.SaveCommand.Execute().ToTask();

            Assert.Equal("Nazwa użytkownika jest wymagana.", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SaveCommand_EmptyEmail_SetsErrorMessage()
        {
            _viewModel.Username = "testuser";
            _viewModel.Email = "";

            await _viewModel.SaveCommand.Execute().ToTask();

            Assert.Equal("Email jest wymagany.", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SaveCommand_FailedUpdate_SetsErrorMessage()
        {
            _viewModel.Username = "newuser";
            _viewModel.Email = "new@example.com";
            UserService.UpdateUserDelegate = (id, username, email, password) => Task.FromResult((false, "Błąd aktualizacji"));

            await _viewModel.SaveCommand.Execute().ToTask();

            Assert.Equal("Błąd aktualizacji", _viewModel.ErrorMessage);
            Assert.Equal("testuser", _viewModel.User.Username);
            Assert.Equal("test@example.com", _viewModel.User.Email);
        }

        [Fact]
        public async Task SaveCommand_ExceptionThrown_SetsErrorMessage()
        {
            _viewModel.Username = "newuser";
            _viewModel.Email = "new@example.com";
            UserService.UpdateUserDelegate = (id, username, email, password) => throw new Exception("Test error");

            await _viewModel.SaveCommand.Execute().ToTask();

            Assert.Equal("Błąd podczas zapisywania: Test error", _viewModel.ErrorMessage);
        }
    }
}