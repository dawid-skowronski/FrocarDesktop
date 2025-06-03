using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class EditCarDialogTests
    {
        [Fact]
        public void Constructor_InitializesComponent()
        {
            // Arrange & Act
            var editCarDialog = new EditCarDialog();

            // Assert
            Assert.NotNull(editCarDialog);
        }
    }
}