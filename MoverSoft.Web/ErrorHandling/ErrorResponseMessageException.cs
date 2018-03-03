namespace MoverSoft.Web.ErrorHandling
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class ErrorResponseMessageException : Exception
    {
        public HttpStatusCode HttpStatus { get; private set; }

        public string ErrorCode { get; private set; }

        public IDictionary<string, string> ResponseHeaders { get; set; }

        public ErrorResponseMessageException(HttpStatusCode httpStatus, string errorCode, string errorMessage, Exception innerException = null, IDictionary<string, string> headers = null)
            : base(errorMessage, innerException)
        {
            this.HttpStatus = httpStatus;
            this.ErrorCode = errorCode;
            this.ResponseHeaders = headers;
        }

        public ErrorResponseMessageException(HttpStatusCode httpStatus, CommonErrorResponseCode errorCode, string errorMessage, Exception innerException = null, IDictionary<string, string> headers = null)
            : this(
                  httpStatus: httpStatus, 
                  errorCode: errorCode.ToString(),
                  errorMessage: errorMessage, 
                  innerException: innerException,
                  headers: headers)
        {
        }
    }
}
