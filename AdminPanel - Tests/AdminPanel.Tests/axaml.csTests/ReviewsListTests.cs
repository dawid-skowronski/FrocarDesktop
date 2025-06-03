using AdminPanel.Views;
using Xunit;

namespace AdminPanel.Tests.axaml.csTests
{
    public class ReviewsListTests
    {
        [Fact]
        public void Constructor_InitializesComponent()
        {
            // Arrange & Act
            var reviewsList = new ReviewsList();

            // Assert
            Assert.Null(reviewsList);
        }
    }
}