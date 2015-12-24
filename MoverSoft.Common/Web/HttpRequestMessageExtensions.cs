using System;
using System.Collections;
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
        public static HttpResponseMessage CreateResponse<T>(
            this HttpRequestMessage request,
            HttpStatusCode statusCode,
            T value,
            HttpConfiguration configuration,
            string encodedContinuationToken) where T : IEnumerable
        {
            ResponseWithContinuation<T> responseWithContinuation = new ResponseWithContinuation<T>
            {
                Value = value,
            };

            if (!string.IsNullOrWhiteSpace(encodedContinuationToken))
            {
                var queryParams = request.RequestUri.ParseQueryString();
                queryParams["$skiptoken"] = encodedContinuationToken;

                var nextLinkBuilder = new UriBuilder(request.Headers.Referrer ?? request.RequestUri);

                nextLinkBuilder.Query = queryParams.ToString();

                responseWithContinuation.NextLink = nextLinkBuilder.Uri.AbsoluteUri;
            }

            return request.CreateResponse(
                statusCode: statusCode,
                value: responseWithContinuation,
                configuration: configuration);
        }
    }
}
