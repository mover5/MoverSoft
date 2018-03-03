namespace MoverSoft.Web.Extensions
{
    using System;
    using System.Linq;
    using MoverSoft.Common.Definitions;
    using MoverSoft.Common.Extensions;

    public static class UriExtensions
    {
        public static InsensitiveDictionary<string> ParseQuery(this Uri source)
        {
            var queryPairs = source.Query.Replace("?", string.Empty).SplitRemoveEmpty("&");
            var queryDictionary = new InsensitiveDictionary<string>();

            foreach (var pair in queryPairs)
            {
                var querySplit = pair.SplitRemoveEmpty("=");
                if (querySplit.Count() == 2)
                {
                    queryDictionary.Add(querySplit[0], querySplit[1]);
                }
            }

            return queryDictionary;
        }
    }
}
