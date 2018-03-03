namespace MoverSoft.Web.Utilities
{
    using System.Net.Http.Formatting;
    using MoverSoft.Common.Extensions;

    public static class JsonMediaTypes
    {
        public static readonly MediaTypeFormatter JsonObjectTypeFormatter = new JsonMediaTypeFormatter { SerializerSettings = JsonExtensions.ObjectSerializationSettings, UseDataContractJsonSerializer = false };

        public static readonly MediaTypeFormatter[] JsonObjectTypeFormatters = new MediaTypeFormatter[] { JsonMediaTypes.JsonObjectTypeFormatter };

        public static readonly MediaTypeFormatter JsonMediaTypeFormatter = new JsonMediaTypeFormatter { SerializerSettings = JsonExtensions.MediaTypeFormatterSettings, UseDataContractJsonSerializer = false };

        public static readonly MediaTypeFormatter[] JsonMediaTypeFormatters = new MediaTypeFormatter[] { JsonMediaTypes.JsonMediaTypeFormatter };
    }
}
