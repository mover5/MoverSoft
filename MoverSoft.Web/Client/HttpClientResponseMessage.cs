namespace MoverSoft.Web.Client
{
    using System.Net;
    using Newtonsoft.Json.Linq;

    public class HttpClientResponseMessage<TValue>
    {
        public TValue Value { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public JToken Error { get; set; }
    }
}
