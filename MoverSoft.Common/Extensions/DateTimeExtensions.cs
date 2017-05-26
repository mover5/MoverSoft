namespace MoverSoft.Common.Extensions
{
    using System;

    public static class DateTimeExtensions
    {
        public static string ToSortableDateTimeString(this DateTime source)
        {
            return source.ToUniversalTime().ToString("yyyyMMddHHmmssfff") + "Z";
        }
    }
}
