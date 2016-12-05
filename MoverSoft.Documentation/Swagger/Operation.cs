using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class Operation
    {
        [JsonProperty]
        public string[] Tags { get; set; }

        [JsonProperty]
        public string Summary { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty]
        public ExternalDocumentation ExternalDocs { get; set; }

        [JsonProperty]
        public string OperationId { get; set; }

        [JsonProperty]
        public string[] Consumes { get; set; }

        [JsonProperty]
        public string[] Produces { get; set; }

        [JsonProperty]
        public Parameter[] Parameters { get; set; }

        [JsonProperty]
        public Dictionary<string, Response> Responses { get; set; }

        [JsonProperty]
        public string[] Schemes { get; set; }

        [JsonProperty]
        public bool Deprecated { get; set; }
    }
}
