using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace MoverSoft.Common.Web
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpResponseMessage CreateResponse<T>(this HttpRequestMessage message, HttpStatusCode statusCode, IEnumerable<T> value, string continuationToken = null, HttpConfiguration configuration = null)
        {
            string nextLink = null;
            if (!string.IsNullOrEmpty(continuationToken))
            {
                var endpoint = message.Headers.Referrer ?? new Uri(message.RequestUri.GetLeftPart(UriPartial.Authority));
                var relativeUri = message.RequestUri.PathAndQuery;
                var seperator = relativeUri.Contains("?") ? "&" : "?";
                relativeUri = string.Format("{0}{1}$skipToken={2}", relativeUri, seperator, continuationToken);
                nextLink = new Uri(endpoint, relativeUri).AbsoluteUri;
            }

            var responseWithContinuation = new ResponseWithContinuation<T>
            {
                Value = value.ToArray(),
                NextLink = nextLink
            };

            return message.CreateResponse(
                statusCode: statusCode,
                value: responseWithContinuation,
                configuration: configuration);
        }
    }
}
