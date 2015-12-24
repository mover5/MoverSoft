
namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http.Formatting;
    using System.Xml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    public static class JsonExtensions
    {
        public const int JsonSerializationMaxDepth = 512;

        public static readonly JsonSerializerSettings ObjectSerializationSettings = new JsonSerializerSettings
        {
            MaxDepth = JsonExtensions.JsonSerializationMaxDepth,
            TypeNameHandling = TypeNameHandling.None,

            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,

            NullValueHandling = NullValueHandling.Ignore,

            ContractResolver = new CamelCasePropertyNamesContractResolver(),

            Converters = new List<JsonConverter>
            {
                new TimeSpanConverter(),
                new StringEnumConverter { CamelCaseText = false },
                new AdjustToUniversalIsoDateTimeConverter(),
            },
        };

        public static readonly JsonSerializerSettings MediaTypeFormatterSettings = new JsonSerializerSettings
        {
            MaxDepth = JsonExtensions.JsonSerializationMaxDepth,
            TypeNameHandling = TypeNameHandling.None,

            DateParseHandling = DateParseHandling.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,

            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,

            ContractResolver = new CamelCasePropertyNamesContractResolver(),

            Converters = new List<JsonConverter>
            {
                new TimeSpanConverter(),
                new StringEnumConverter { CamelCaseText = false },
                new AdjustToUniversalIsoDateTimeConverter(),
            },
        };

        public static readonly JsonSerializer JsonObjectTypeSerializer = JsonSerializer.Create(JsonExtensions.ObjectSerializationSettings);

        public static readonly JsonSerializer JsonMediaTypeSerializer = JsonSerializer.Create(JsonExtensions.MediaTypeFormatterSettings);

        public static readonly MediaTypeFormatter JsonObjectTypeFormatter = new JsonMediaTypeFormatter { SerializerSettings = JsonExtensions.ObjectSerializationSettings, UseDataContractJsonSerializer = false };

        public static readonly MediaTypeFormatter[] JsonObjectTypeFormatters = new MediaTypeFormatter[] { JsonExtensions.JsonObjectTypeFormatter };

        public static readonly MediaTypeFormatter JsonMediaTypeFormatter = new JsonMediaTypeFormatter { SerializerSettings = JsonExtensions.MediaTypeFormatterSettings, UseDataContractJsonSerializer = false };

        public static readonly MediaTypeFormatter[] JsonMediaTypeFormatters = new MediaTypeFormatter[] { JsonExtensions.JsonMediaTypeFormatter };

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonExtensions.ObjectSerializationSettings);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonExtensions.ObjectSerializationSettings);
        }

        public static T FromJson<T>(this string json, JsonSerializerSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, JsonExtensions.ObjectSerializationSettings);
        }

        public static JToken ToJToken(this object obj)
        {
            return JToken.FromObject(obj, JsonExtensions.JsonObjectTypeSerializer);
        }

        public static T FromJToken<T>(this JToken jtoken)
        {
            return jtoken.ToObject<T>(JsonExtensions.JsonObjectTypeSerializer);
        }

        public static object FromJToken(this JToken jtoken, Type type)
        {
            return jtoken.ToObject(type, JsonExtensions.JsonObjectTypeSerializer);
        }

        public static JToken GetProperty(this JToken entity, string propertyName)
        {
            JToken value;

            JObject container = entity as JObject;
            return container != null && container.TryGetValue(propertyName, StringComparison.InvariantCultureIgnoreCase, out value)
                ? value
                : null;
        }

        public static T GetProperty<T>(this JToken entity, string propertyName)
        {
            var targetProperty = entity.GetProperty(propertyName);
            return targetProperty != null ? targetProperty.FromJToken<T>() : default(T);
        }

        internal class TimeSpanConverter : JsonConverter
        {
            /// <summary>
            /// Writes the <c>JSON</c>.
            /// </summary>
            /// <param name="writer">The writer.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, XmlConvert.ToString((TimeSpan)value));
            }

            /// <summary>
            /// Reads the <c>JSON</c>.
            /// </summary>
            /// <param name="reader">The reader.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">The existing value.</param>
            /// <param name="serializer">The serializer.</param>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return reader.TokenType != JsonToken.Null ? (object)XmlConvert.ToTimeSpan(serializer.Deserialize<string>(reader)) : null;
            }

            /// <summary>
            /// Determines whether this instance can convert the specified object type.
            /// </summary>
            /// <param name="objectType">Type of the object.</param>
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(TimeSpan) || objectType == typeof(TimeSpan?);
            }
        }

        internal class AdjustToUniversalIsoDateTimeConverter : IsoDateTimeConverter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AdjustToUniversalIsoDateTimeConverter"/> class.
            /// </summary>
            public AdjustToUniversalIsoDateTimeConverter()
            {
                this.DateTimeStyles = DateTimeStyles.AdjustToUniversal;
                this.Culture = CultureInfo.InvariantCulture;
            }
        }
    }
}
