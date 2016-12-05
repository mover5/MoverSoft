using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Schema
    {
        [JsonProperty(PropertyName = "$ref")]
        public string Ref { get; set; }

        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string Format { get; set; }

        [JsonProperty]
        public Schema Items { get; set; }

        [JsonProperty(PropertyName = "required")]
        public string[] RequiredFields { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string[] Enum { get; set; }

        [JsonProperty]
        public Dictionary<string, Schema> Properties { get; set; }

        [JsonProperty]
        public Schema AdditionalProperties { get; set; }
    }
}
