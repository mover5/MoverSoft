using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Response
    {
        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public Schema Schema { get; set; }

        [JsonProperty]
        public Dictionary<string, Header> Headers { get; set; }
    }
}
