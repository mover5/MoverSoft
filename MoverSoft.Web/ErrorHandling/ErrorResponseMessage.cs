
namespace MoverSoft.Web.ErrorHandling
{
    using System;
    using Newtonsoft.Json;

    public class ErrorResponseMessage
    {
        [JsonProperty]
        public string Message { get; set; }

        [JsonProperty]
        public string Code { get; set; }

        [JsonProperty]
        public Exception Exception { get; set; }
    }
}
