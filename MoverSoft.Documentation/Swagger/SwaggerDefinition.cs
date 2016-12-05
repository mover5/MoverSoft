using System.Collections.Generic;
using Newtonsoft.Json;

namespace MoverSoft.Documentation.Swagger
{
    public class SwaggerDefinition
    {
        [JsonProperty(PropertyName = "swagger")]
        public string SwaggerVersion
        {
            get { return "2.0"; }
        }

        [JsonProperty]
        public SwaggerInfo Info { get; set; }

        [JsonProperty]
        public string Host { get; set; }

        [JsonProperty]
        public string BasePath { get; set; }

        [JsonProperty]
        public string[] Schemes { get; set; }

        [JsonProperty]
        public string[] Consumes { get; set; }

        [JsonProperty]
        public string[] Produces { get; set; }

        [JsonProperty]
        public Dictionary<string, Path> Paths { get; set; }

        [JsonProperty]
        public Dictionary<string, Schema> Definitions { get; set; }

        [JsonProperty]
        public Dictionary<string, Parameter> Parameters { get; set; }

        [JsonProperty]
        public Dictionary<string, SecurityDefinition> SecurityDefinitions { get; set; }

        [JsonProperty]
        public Dictionary<string, string[]>[] Security { get; set; }

        [JsonProperty]
        public Tag[] Tags { get; set; }

        [JsonProperty]
        public ExternalDocumentation ExternalDocs { get; set; }
    }
}
