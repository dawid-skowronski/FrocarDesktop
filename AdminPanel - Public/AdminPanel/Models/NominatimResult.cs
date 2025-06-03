using System.Text.Json.Serialization;

namespace AdminPanel.Models
{
    public class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string? Latitude { get; set; }

        [JsonPropertyName("lon")]
        public string? Longitude { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}