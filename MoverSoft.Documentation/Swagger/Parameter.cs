using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Parameter
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string In { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string Format { get; set; }

        [JsonProperty]
        public bool Required { get; set; }

        [JsonProperty]
        public Schema Items { get; set; }

        [JsonProperty]
        public string CollectionFormat { get; set; }

        [JsonProperty]
        public Schema Schema { get; set; }
    }
}
