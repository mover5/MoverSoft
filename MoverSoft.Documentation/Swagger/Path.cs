using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Path
    {
        [JsonProperty]
        public Operation Get { get; set; }

        [JsonProperty]
        public Operation Put { get; set; }

        [JsonProperty]
        public Operation Post { get; set; }

        [JsonProperty]
        public Operation Delete { get; set; }

        [JsonProperty]
        public Operation Options { get; set; }

        [JsonProperty]
        public Operation Head { get; set; }

        [JsonProperty]
        public Operation Patch { get; set; }
    }
}
