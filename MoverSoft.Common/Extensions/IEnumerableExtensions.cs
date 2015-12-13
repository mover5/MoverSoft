namespace MoverSoft.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> CoalesceEnumerable<T>(this IEnumerable<T> source)
        {
            return source ?? new T[0];
        }

        public static T[] AsArray<T>(this T source)
        {
            return new T[] { source };
        }

        public static TResult[] SelectArray<T, TResult>(this IEnumerable<T> source, Func<T, TResult> selector)
        {
            return source.Select(selector).ToArray();
        }

        public static TResult[] SelectManyArray<T, TResult>(this IEnumerable<T> source, Func<T, TResult[]> selector)
        {
            return source.SelectMany(selector).ToArray();
        }

        public static IEnumerable<T> DistinctArray<T>(this IEnumerable<T> source)
        {
            return source.Distinct().ToArray();
        }

        public static IEnumerable<T> DistinctArray<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return source.Distinct(comparer).ToArray();
        }
    }
}
