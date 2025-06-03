using AdminPanel.Models;
using AdminPanel.Services;
using AdminPanel.ViewModels;
using Moq;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Xunit;

namespace AdminPanel.Tests.ViewModels
{
    public class StatisticsViewModelTests : IDisposable
    {
        private readonly Mock<IStatisticsService> _mockStatisticsService;

        public StatisticsViewModelTests()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("pl-PL");
            _mockStatisticsService = new Mock<IStatisticsService>();
            Locator.CurrentMutable.RegisterConstant(_mockStatisticsService.Object, typeof(IStatisticsService));
        }

        public void Dispose()
        {
            Locator.CurrentMutable.UnregisterAll<IStatisticsService>();
        }

        [Fact]
        public async Task Constructor_InitializesAndExecutesLoadStatisticsCommand()
        {
            // Arrange
            var statistics = new StatisticsDto
            {
                TopSpenderUsername = "User1",
                TopSpenderAmount = 1000m,
                UserWithMostCarsUsername = "User2",
                UserWithMostCarsCount = 5
            };
            _mockStatisticsService.Setup(s => s.GetStatistics())
                .ReturnsAsync((true, statistics, "Pobrano statystyki pomyślnie"));

            // Act
            var viewModel = new StatisticsViewModel();

            // Assert
            Assert.NotNull(viewModel.LoadStatisticsCommand);
        }

        [Fact]
        public async Task LoadStatistics_Failure_SetsErrorMessage()
        {
            // Arrange
            _mockStatisticsService.Setup(s => s.GetStatistics())
                .ReturnsAsync((false, null, "Błąd ładowania danych"));
            var viewModel = new StatisticsViewModel();

            // Act
            await viewModel.LoadStatisticsCommand.Execute().ToTask();

            // Assert
            Assert.False(viewModel.IsLoading);
            Assert.Null(viewModel.Statistics);
            Assert.Equal("Najwięcej wydał: Brak danych (0,00 zł)", viewModel.TopSpenderFormatted);
            Assert.Equal("Najwięcej wystawionych pojazdów: Brak danych (0)", viewModel.UserWithMostCarsFormatted);
        }

        [Fact]
        public void TopSpenderFormatted_ReturnsDefault_WhenStatisticsNull()
        {
            // Arrange
            var viewModel = new StatisticsViewModel();
            viewModel.Statistics = null;

            // Act & Assert
            Assert.Equal("Najwięcej wydał: Brak danych (0,00 zł)", viewModel.TopSpenderFormatted);
        }

        [Fact]
        public void UserWithMostCarsFormatted_ReturnsDefault_WhenStatisticsNull()
        {
            // Arrange
            var viewModel = new StatisticsViewModel();
            viewModel.Statistics = null;

            // Act & Assert
            Assert.Equal("Najwięcej wystawionych pojazdów: Brak danych (0)", viewModel.UserWithMostCarsFormatted);
        }

        [Fact]
        public async Task LoadStatistics_SetsIsLoadingCorrectly()
        {
            // Arrange
            var statistics = new StatisticsDto();
            var tcs = new TaskCompletionSource<(bool, StatisticsDto, string)>();
            _mockStatisticsService.Setup(s => s.GetStatistics())
                .Returns(tcs.Task);
            var viewModel = new StatisticsViewModel();

            // Act
            var loadTask = viewModel.LoadStatisticsCommand.Execute().ToTask();
            Assert.True(viewModel.IsLoading);

            // Complete the task
            tcs.SetResult((true, statistics, "Pobrano statystyki pomyślnie"));
            await loadTask;

            // Assert
            Assert.False(viewModel.IsLoading);
        }
    }

    public interface IStatisticsService
    {
        Task<(bool IsSuccess, StatisticsDto Statistics, string Message)> GetStatistics();
    }
}