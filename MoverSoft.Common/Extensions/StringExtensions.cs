
namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class StringExtensions
    {
        public static string CoaleseString(this string source)
        {
            return source ?? string.Empty;
        }

        public static bool EqualsInsensitively(this string original, string otherString)
        {
            return string.Equals(original, otherString, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool StartsWithInsensitively(this string original, string prefix)
        {
            return original.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EndsWithInsensitively(this string original, string suffix)
        {
            return original.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ContainsInsensitively(this IEnumerable<string> source, string value)
        {
            return source.Contains(value, StringComparer.InvariantCultureIgnoreCase);
        }

        public static bool ContainsInsensitively(this string source, string value)
        {
            return source.LastIndexOf(value, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public static bool EqualsSensitively(this string original, string otherString)
        {
            return string.Equals(original, otherString, StringComparison.InvariantCulture);
        }

        public static string DecodeFromBase64String(this string original)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(original));
        }

        public static string EncodeToBase64String(this string original)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(original));
        }

        public static string ConcatStrings(this IEnumerable<string> strings, char separator)
        {
            return strings.ConcatStrings(separator.ToString());
        }

        public static string ConcatStrings(this string[] strings, char separator)
        {
            return strings.ConcatStrings(separator.ToString());
        }

        public static string ConcatStrings(this IEnumerable<string> strings, string separator = "")
        {
            return string.Join(separator, strings);
        }

        public static string ConcatStrings(this string[] strings, string separator = "")
        {
            return string.Join(separator, strings);
        }

        public static string[] SplitRemoveEmpty(this string source, params char[] seperator)
        {
            return source.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitRemoveEmpty(this string source, params string[] seperator)
        {
            return source.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
