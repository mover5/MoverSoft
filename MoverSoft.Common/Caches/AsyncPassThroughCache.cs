namespace MoverSoft.Common.Caches
{
    using System;
    using System.Threading.Tasks;
    using MoverSoft.Common.Definitions;

    public class AsyncPassThroughCache<TValue> where TValue : class
    {
        protected class CacheItem<TCacheValue>
        {
            public TCacheValue Value { get; set; }
            public DateTime? ExpirationTime { get; set; }
        }

        private InsensitiveDictionary<CacheItem<TValue>> CacheData { get; set; }

        public AsyncPassThroughCache()
        {
            this.CacheData = new InsensitiveDictionary<CacheItem<TValue>>();
        }

        public TValue PopValue(string cacheKey)
        {
            var value = this.GetValue(cacheKey);
            this.RemoveValue(cacheKey);

            return value;
        }

        public void RemoveValue(string cacheKey)
        {
            if (this.CacheData.ContainsKey(cacheKey))
            {
                this.CacheData.Remove(cacheKey);
            }
        }

        public void SetValue(
            string cacheKey, 
            TValue value, 
            TimeSpan? cacheItemExpiry = null)
        {
            if (value != null)
            {
                var data = new CacheItem<TValue>
                {
                    Value = value
                };

                if (cacheItemExpiry.HasValue)
                {
                    data.ExpirationTime = DateTime.UtcNow.Add(cacheItemExpiry.Value);
                }

                this.CacheData[cacheKey] = data;
            }
        }

        public TValue GetValue(string cacheKey)
        {
            if (this.CacheData.ContainsKey(cacheKey))
            {
                var valueObject = this.CacheData[cacheKey];

                // If the value is expired, remove it from the cache and return null
                if (valueObject.ExpirationTime.HasValue && valueObject.ExpirationTime.Value < DateTime.UtcNow)
                {
                    this.RemoveValue(cacheKey);
                    return null;
                }

                return valueObject.Value;
            }

            return null;
        }

        public TValue GetValue(
            string cacheKey, 
            Func<TValue> valueFactory = null, 
            TimeSpan? cacheItemExpiry = null)
        {
            var value = this.GetValue(cacheKey);

            if (value == null && valueFactory != null)
            {
                value = valueFactory();

                if (value != null)
                {
                    this.SetValue(
                        cacheKey: cacheKey,
                        value: value,
                        cacheItemExpiry: cacheItemExpiry);
                }
            }

            return value;
        }

        public async Task<TValue> GetValue(
            string cacheKey, 
            Func<Task<TValue>> valueFactory = null, 
            TimeSpan? cacheItemExpiry = null)
        {
            var value = this.GetValue(cacheKey);

            if (value == null && valueFactory != null)
            {
                value = await valueFactory();

                if (value != null)
                {
                    this.SetValue(
                        cacheKey: cacheKey,
                        value: value,
                        cacheItemExpiry: cacheItemExpiry);
                }
            }

            return value;
        }
    }
}
