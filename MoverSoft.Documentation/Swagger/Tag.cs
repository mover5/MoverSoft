using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Tag
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Description { get; set; }
    }
}
