using System;

namespace MoverSoft.Documentation.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequestQueryAttribute : Attribute
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Required { get; set; }

        public RequestQueryAttribute()
        {
        }
    }
}
