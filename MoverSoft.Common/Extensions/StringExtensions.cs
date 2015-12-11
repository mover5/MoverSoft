namespace MoverSoft.Common.Extensions
{
    public static class StringExtensions
    {
        public static string CoaleseString(this string source)
        {
            return source ?? string.Empty;
        }
    }
}
