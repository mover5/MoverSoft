namespace MoverSoft.Common.Data
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using MoverSoft.Common.Extensions;
    using Newtonsoft.Json.Linq;

    public class HttpClientDataProvider
    {
        private HttpClient httpClient { get; set; }

        public HttpClientDataProvider(string rootDomain)
        {
            if (!rootDomain.ContainsInsensitively("http"))
            {
                rootDomain = string.Format("https://{0}", rootDomain);
            }

            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(rootDomain);
        }

        protected Task<HttpClientResponseMessage<T>> SendRequest<T>(string authToken, string relativeUri, HttpMethod method)
        {
            return this.SendRequest<T, object>(authToken, relativeUri, method);
        }

        protected async Task<HttpClientResponseMessage<T>> SendRequest<T, U>(string authToken, string relativeUri, HttpMethod method, U body = null) where U : class        {
            var requestMessage = new HttpRequestMessage(method, relativeUri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            if (body != null)
            {
                requestMessage.Content = new StringContent(body.ToJson(), Encoding.UTF8, "application/json");
            }

            var response = await this.httpClient.SendAsync(requestMessage);

            var clientResponse = new HttpClientResponseMessage<T>
            {
                StatusCode = response.StatusCode
            };

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    clientResponse.Value = await response.Content
                        .ReadAsAsync<T>(JsonExtensions.JsonMediaTypeFormatters)
                        .ConfigureAwait(continueOnCapturedContext: false);
                }
                catch (Exception)
                {
                    clientResponse.Value = default(T);
                    clientResponse.Error = new JObject
                    {
                        "Error", "Response did not deserialize correctly"
                    };
                }
            }
            else
            {
                clientResponse.Error = await response.Content
                    .ReadAsAsync<JToken>(JsonExtensions.JsonMediaTypeFormatters)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }

            return clientResponse;
        }
    }
}
