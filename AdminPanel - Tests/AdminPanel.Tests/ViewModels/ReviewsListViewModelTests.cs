using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public sealed class ReviewsListViewModelTests : IDisposable
    {
        private readonly MockReviewService _mockReviewService;
        private readonly MockTokenService _mockTokenService;

        public ReviewsListViewModelTests()
        {
            _mockReviewService = new MockReviewService();
            _mockTokenService = new MockTokenService();
        }

        public void Dispose()
        {
            // Przywracanie oryginalnych delegacji
            ReviewService.GetReviewsDelegate = null;
            ReviewService.DeleteReviewDelegate = null;
        }

        [Fact]
        public async Task Constructor_InitializesAndLoadsReviews()
        {
            // Arrange
            var reviews = new List<ReviewDto>
            {
                new ReviewDto { ReviewId = 1, UserId = 1, Rating = 5, Comment = "Great!", CreatedAt = DateTime.Now }
            };
            _mockReviewService.SetGetReviewsResult(true, reviews, "Pobrano oceny");

            // Act
            var viewModel = new ReviewsListViewModel();

            // Assert
            await Task.Delay(100); // Czekaj na wykonanie asynchronicznej komendy
            Assert.True(_mockReviewService.GetReviewsCalled);
            Assert.Single(viewModel.Reviews);
            Assert.Equal(1, viewModel.Reviews[0].ReviewId);
            Assert.NotNull(viewModel.Reviews[0].DeleteCommand);
            Assert.False(viewModel.IsLoading);
            Assert.Null(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadReviewsCommand_LoadsAndSortsReviews()
        {
            // Arrange
            var reviews = new List<ReviewDto>
            {
                new ReviewDto { ReviewId = 1, UserId = 1, Rating = 5, Comment = "Great!", CreatedAt = DateTime.Now.AddDays(-1) },
                new ReviewDto { ReviewId = 2, UserId = 2, Rating = 3, Comment = "Okay", CreatedAt = DateTime.Now }
            };
            _mockReviewService.SetGetReviewsResult(true, reviews, "Pobrano oceny");
            var viewModel = new ReviewsListViewModel();

            // Act
            await viewModel.LoadReviewsCommand.Execute().ToTask();

            // Assert
            Assert.True(_mockReviewService.GetReviewsCalled);
            Assert.Equal(2, viewModel.Reviews.Count);
            Assert.Equal(2, viewModel.Reviews[0].ReviewId); // Najnowsza recenzja pierwsza
            Assert.Equal(1, viewModel.Reviews[1].ReviewId);
            Assert.False(viewModel.IsLoading);
            Assert.Null(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadReviewsCommand_HandlesError()
        {
            // Arrange
            _mockReviewService.SetGetReviewsResult(false, null, "Błąd serwera");
            var viewModel = new ReviewsListViewModel();

            // Act
            await viewModel.LoadReviewsCommand.Execute().ToTask();

            // Assert
            Assert.True(_mockReviewService.GetReviewsCalled);
            Assert.Empty(viewModel.Reviews);
            Assert.Equal("Błąd serwera", viewModel.ErrorMessage);
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task LoadReviewsCommand_HandlesException()
        {
            // Arrange
            var exception = new Exception("Connection failed");
            _mockReviewService.SetGetReviewsException(exception);
            var viewModel = new ReviewsListViewModel();

            // Act
            await viewModel.LoadReviewsCommand.Execute().ToTask();

            // Assert
            Assert.True(_mockReviewService.GetReviewsCalled);
            Assert.Empty(viewModel.Reviews);
            Assert.Equal("Błąd: Connection failed", viewModel.ErrorMessage);
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task RefreshCommand_ReloadsReviews()
        {
            // Arrange
            var reviews = new List<ReviewDto>
            {
                new ReviewDto { ReviewId = 1, UserId = 1, Rating = 5, Comment = "Great!", CreatedAt = DateTime.Now }
            };
            _mockReviewService.SetGetReviewsResult(true, reviews, "Pobrano oceny");
            var viewModel = new ReviewsListViewModel();
            await viewModel.LoadReviewsCommand.Execute().ToTask();

            var newReviews = new List<ReviewDto>
            {
                new ReviewDto { ReviewId = 2, UserId = 2, Rating = 4, Comment = "Good", CreatedAt = DateTime.Now }
            };
            _mockReviewService.SetGetReviewsResult(true, newReviews, "Pobrano oceny");
            _mockReviewService.ResetGetReviewsCalled();

            // Act
            await viewModel.RefreshCommand.Execute().ToTask();

            // Assert
            Assert.True(_mockReviewService.GetReviewsCalled);
            Assert.Single(viewModel.Reviews);
            Assert.Equal(2, viewModel.Reviews[0].ReviewId);
            Assert.False(viewModel.IsLoading);
            Assert.Null(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task DeleteCommand_RemovesReviewOnSuccess()
        {
            // Arrange
            var reviews = new List<ReviewDto>
            {
                new ReviewDto { ReviewId = 1, UserId = 1, Rating = 5, Comment = "Great!", CreatedAt = DateTime.Now }
            };
            _mockReviewService.SetGetReviewsResult(true, reviews, "Pobrano oceny");
            _mockReviewService.SetDeleteReviewResult(1, true, "Usunięto");
            var viewModel = new ReviewsListViewModel();
            await viewModel.LoadReviewsCommand.Execute().ToTask();

            // Act
            await viewModel.Reviews[0].DeleteCommand.Execute(1).ToTask();

            // Assert
            Assert.True(_mockReviewService.DeleteReviewCalled);
            Assert.Empty(viewModel.Reviews);
            Assert.Null(viewModel.ErrorMessage);
            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task DeleteCommand_SetsErrorMessageOnFailure()
        {
            // Arrange
            var reviews = new List<ReviewDto>
            {
                new ReviewDto { ReviewId = 1, UserId = 1, Rating = 5, Comment = "Great!", CreatedAt = DateTime.Now }
            };
            _mockReviewService.SetGetReviewsResult(true, reviews, "Pobrano oceny");
            _mockReviewService.SetDeleteReviewResult(1, false, "Błąd usuwania");
            var viewModel = new ReviewsListViewModel();
            await viewModel.LoadReviewsCommand.Execute().ToTask();

            // Act
            await viewModel.Reviews[0].DeleteCommand.Execute(1).ToTask();

            // Assert
            Assert.True(_mockReviewService.DeleteReviewCalled);
            Assert.Single(viewModel.Reviews);
            Assert.Equal("Błąd usuwania", viewModel.ErrorMessage);
            Assert.False(viewModel.IsLoading);
        }
    }

    internal class MockReviewService
    {
        private Func<Task<(bool, List<ReviewDto>, string)>> _getReviewsFunc;
        private Func<int, Task<(bool, string)>> _deleteReviewFunc;
        public bool GetReviewsCalled { get; private set; }
        public bool DeleteReviewCalled { get; private set; }

        public void SetGetReviewsResult(bool isSuccess, List<ReviewDto> reviews, string message)
        {
            _getReviewsFunc = () =>
            {
                GetReviewsCalled = true;
                return Task.FromResult((isSuccess, reviews, message));
            };
            ReviewService.GetReviewsDelegate = _getReviewsFunc;
        }

        public void SetGetReviewsException(Exception ex)
        {
            _getReviewsFunc = () =>
            {
                GetReviewsCalled = true;
                throw ex;
            };
            ReviewService.GetReviewsDelegate = _getReviewsFunc;
        }

        public void SetDeleteReviewResult(int reviewId, bool isSuccess, string message)
        {
            _deleteReviewFunc = (id) =>
            {
                DeleteReviewCalled = true;
                return Task.FromResult((isSuccess, message));
            };
            ReviewService.DeleteReviewDelegate = _deleteReviewFunc;
        }

        public void ResetGetReviewsCalled()
        {
            GetReviewsCalled = false;
        }
    }
}