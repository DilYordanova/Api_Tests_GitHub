using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace APITestsGitHub
{
    public class Location
    {
        [JsonPropertyName("country abbreviation")]
        public string Abbreviation { get; set; }

        [JsonPropertyName("post code")]
        public string PostCode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("places")]
        public List<Places> Places { get; set; }

    }

    public class Places
    {
        [JsonPropertyName("place name")]
        public string PlaceName { get; set; }
        public string Longitude { get; set; }
        public string State { get; set; }
        public string State_abbreviation { get; set; }
        public string Latitude { get; set; }

    }
}
