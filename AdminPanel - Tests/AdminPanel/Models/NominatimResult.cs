using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace AdminPanel.Models
{
    public class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; }

        [JsonPropertyName("lon")]
        public string Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }
    }
}
