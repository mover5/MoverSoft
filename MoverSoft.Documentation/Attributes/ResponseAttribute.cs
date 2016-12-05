using System;
using System.Net;

namespace MoverSoft.Documentation.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ResponseAttribute : Attribute
    {
        public string Description { get; set; }

        public readonly Type ResponseType;

        public readonly HttpStatusCode ExpectedStatusCode;

        public ResponseAttribute(Type responseType, HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            this.ResponseType = responseType;
            this.ExpectedStatusCode = expectedStatusCode;
        }
    }
}
