namespace MoverSoft.Web.ErrorHandling
{
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Filters;

    public class ErrorResponseFilter : ExceptionFilterAttribute
    {
        public HttpConfiguration HttpConfiguration { get; set; }

        public ErrorResponseFilter(HttpConfiguration configuration)
        {
            this.HttpConfiguration = configuration;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is ErrorResponseMessageException)
            {
                var errorException = actionExecutedContext.Exception as ErrorResponseMessageException;
                var errorResponse = new ErrorResponseMessage
                {
                    Message = errorException.Message,
                    Code = errorException.ErrorCode,
                    Exception = errorException.InnerException
                };

                var response = actionExecutedContext.Request.CreateResponse(
                    statusCode: errorException.HttpStatus,
                    value: errorResponse,
                    configuration: this.HttpConfiguration);

                if (errorException.ResponseHeaders != null && errorException.ResponseHeaders.Count > 0)
                {
                    foreach (var headerKVP in errorException.ResponseHeaders)
                    {
                        response.Headers.Add(headerKVP.Key, headerKVP.Value);
                    }
                }

                actionExecutedContext.Response = response;
            }
            else
            {
                base.OnException(actionExecutedContext);
            }
        }
    }
}
