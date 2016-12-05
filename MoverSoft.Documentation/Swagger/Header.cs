using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Header
    {
        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string Type { get; set; }

        [JsonProperty]
        public string Format { get; set; }

        [JsonProperty]
        public string CollectionFormat { get; set; }
    }
}
