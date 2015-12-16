
namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http.Formatting;
    using System.Xml;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
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

        public static readonly JsonSerializerSettings MediaJsonSerializationSettings = new JsonSerializerSettings
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

        public static string ToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, JsonExtensions.ObjectSerializationSettings);
        }

        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, JsonExtensions.ObjectSerializationSettings);
        }

        public static object FromJson(this string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, JsonExtensions.ObjectSerializationSettings);
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
