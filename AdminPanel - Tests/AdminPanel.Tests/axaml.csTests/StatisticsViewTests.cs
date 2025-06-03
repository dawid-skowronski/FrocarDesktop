using AdminPanel.ViewModels;
using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class StatisticsViewTests
    {
        [Fact]
        public void Constructor_SetsDataContextToStatisticsViewModel()
        {
            // Arrange & Act
            var statisticsView = new StatisticsView();

            // Assert
            Assert.NotNull(statisticsView.DataContext);
            Assert.IsType<StatisticsViewModel>(statisticsView.DataContext);
        }
    }
}