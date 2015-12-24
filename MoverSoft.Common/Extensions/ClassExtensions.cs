namespace MoverSoft.Common.Extensions
{
    public static class ClassExtensions
    {
        public static T Coalesce<T>(this T source) where T : new()
        {
            return source != null ? source : new T();
        }

        public static T Cast<T>(this object obj)
        {
            return (T)obj;
        }
    }
}
