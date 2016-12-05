using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class ExternalDocumentation
    {
        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public string Url { get; set; }
    }
}
