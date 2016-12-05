using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class SecurityDefinition
    {
        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string In { get; set; }

        [JsonProperty]
        public string Flow { get; set; }

        [JsonProperty]
        public string AuthorizationUrl { get; set; }

        [JsonProperty]
        public string TokenUrl { get; set; }

        [JsonProperty]
        public Dictionary<string, string> Scopes { get; set; }
    }
}
