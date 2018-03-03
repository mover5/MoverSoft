namespace MoverSoft.Web.Utilities
{
    using System;
    using MoverSoft.Common.Definitions;
    using MoverSoft.Web.Extensions;

    public class QueryStringReader
    {
        public InsensitiveDictionary<string> QueryParameters { get; private set; }

        public const string FilterQuery = "$filter";

        public const string SearchQuery = "$search";

        public const string SkipTokenQuery = "$skipToken";

        public QueryStringReader(Uri requestUri)
        {
            this.QueryParameters = requestUri.ParseQuery();
        }

        public string Filter
        {
            get
            {
                if (this.QueryParameters != null && this.QueryParameters.ContainsKey(QueryStringReader.FilterQuery))
                {
                    return this.QueryParameters[QueryStringReader.FilterQuery];
                }

                return null;
            }
        }

        public string Search
        {
            get
            {
                if (this.QueryParameters != null && this.QueryParameters.ContainsKey(QueryStringReader.SearchQuery))
                {
                    return this.QueryParameters[QueryStringReader.SearchQuery];
                }

                return null;
            }
        }

        public string SkipToken
        {
            get
            {
                if (this.QueryParameters != null && this.QueryParameters.ContainsKey(QueryStringReader.SkipTokenQuery))
                {
                    return this.QueryParameters[QueryStringReader.SkipTokenQuery];
                }

                return null;
            }
        }
    }
}
