namespace MoverSoft.Common.Extensions
{
    public static class StringExtensions
    {
        public static string CoaleseString(this string source)
        {
            return source ?? string.Empty;
        }

        public static string ConcatStrings(this string[] strings, string separator = "")
        {
            return string.Join(separator, strings);
        }
    }
}
