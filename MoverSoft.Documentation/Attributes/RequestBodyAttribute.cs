using System;

namespace MoverSoft.Documentation.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequestBodyAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public readonly Type RequestBodyType;

        public RequestBodyAttribute(Type requestBodyType)
        {
            this.RequestBodyType = requestBodyType;
        }
    }
}
