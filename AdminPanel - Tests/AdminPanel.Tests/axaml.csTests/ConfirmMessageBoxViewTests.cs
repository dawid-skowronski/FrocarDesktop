using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class ConfirmMessageBoxViewTests
    {
        [Fact]
        public void Constructor_SetsDataContextToSelf()
        {
            // Arrange & Act
            var confirmMessageBox = new ConfirmMessageBoxView();

            // Assert
            Assert.NotNull(confirmMessageBox.DataContext);
            Assert.Same(confirmMessageBox, confirmMessageBox.DataContext);
            Assert.NotNull(confirmMessageBox.Result);
        }

        [Fact]
        public void Constructor_WithTitle_SetsTitleAndDataContext()
        {
            // Arrange
            const string title = "Test Title";

            // Act
            var confirmMessageBox = new ConfirmMessageBoxView(title);

            // Assert
            Assert.Equal(title, confirmMessageBox.Title);
            Assert.NotNull(confirmMessageBox.DataContext);
            Assert.Same(confirmMessageBox, confirmMessageBox.DataContext);
            Assert.NotNull(confirmMessageBox.Result);
        }
    }
}