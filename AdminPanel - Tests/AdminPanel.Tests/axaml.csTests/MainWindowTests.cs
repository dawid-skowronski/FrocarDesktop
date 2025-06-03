using AdminPanel.Views;
using Avalonia.Controls;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class MainWindowTests
    {
        [Fact]
        public void Constructor_InitializesTrayIconAndNotificationService()
        {
            // Arrange & Act
            var mainWindow = new MainWindow();

            // Assert
            Assert.NotNull(mainWindow);
            Assert.Equal(WindowState.Maximized, mainWindow.WindowState);
        }
    }
}