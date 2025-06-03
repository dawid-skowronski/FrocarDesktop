using AdminPanel.Models;
using Xunit;

namespace AdminPanel.Tests.Models
{
    public class NominatimResultTests
    {
        [Fact]
        public void NominatimResult_Properties_SetAndGetCorrectly()
        {
            // Arrange
            var result = new NominatimResult
            {
                Lat = "52.5200",
                Lon = "13.4050",
                DisplayName = "Berlin, Germany"
            };

            // Act & Assert
            Assert.Equal("52.5200", result.Lat);
            Assert.Equal("13.4050", result.Lon);
            Assert.Equal("Berlin, Germany", result.DisplayName);
        }

        [Fact]
        public void NominatimResult_DefaultValues_AreNull()
        {
            // Arrange
            var result = new NominatimResult();

            // Act & Assert
            Assert.Null(result.Lat);
            Assert.Null(result.Lon);
            Assert.Null(result.DisplayName);
        }
    }
}