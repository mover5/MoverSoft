namespace MoverSoft.Web.ErrorHandling
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using MoverSoft.Common.Extensions;

    public class ErrorResponseHandler : DelegatingHandler
    {
        private HttpConfiguration HttpConfiguration { get; set; }

        public ErrorResponseHandler(HttpConfiguration httpConfiguration)
        {
            this.HttpConfiguration = httpConfiguration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Exception exception)
            {
                if (exception.IsFatal())
                {
                    throw;
                }

                ErrorResponseMessage errorResponse = null;
                HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

                if (exception.GetType() == typeof(ErrorResponseMessageException))
                {
                    var errorException = exception as ErrorResponseMessageException;
                    errorResponse = new ErrorResponseMessage
                    {
                        Message = errorException.Message,
                        Code = errorException.ErrorCode,
                        Exception = errorException.InnerException
                    };

                    statusCode = errorException.HttpStatus;
                }
                else
                {
                    errorResponse = new ErrorResponseMessage
                    {
                        Message = "An error occured",
                        Code = CommonErrorResponseCode.InternalServerError.ToString(),
                        Exception = exception
                    };
                }

                return request.CreateResponse(
                    statusCode: statusCode,
                    value: errorResponse,
                    configuration: this.HttpConfiguration);
            }
        }
    }
}
