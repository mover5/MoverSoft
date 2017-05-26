using System;
using System.Threading.Tasks;
using MoverSoft.Common.Utilities;

namespace MoverSoft.Common.Caches
{
    public class AsyncPassthroughCache<T> where T : class
    {
        protected class CacheRecord<TCacheValue>
        {
            public TCacheValue Value { get; set; }
            public DateTime? ExpirationTime { get; set; }
        }

        protected InsensitiveDictionary<CacheRecord<T>> Cache { get; set; }

        public AsyncPassthroughCache()
        {
            this.Cache = new InsensitiveDictionary<CacheRecord<T>>();
        }

        public int CacheSize()
        {
            return this.Cache.Count;
        }

        public virtual void AddItem(string key, T item, TimeSpan? expiration = null)
        {
            DateTime? expirationTime = expiration.HasValue ? (DateTime?)DateTime.UtcNow.Add(expiration.Value) : null;
            var cacheItem = new CacheRecord<T>
            {
                Value = item,
                ExpirationTime = expirationTime
            };

            if (this.Cache.ContainsKey(key))
            {
                this.Cache.Remove(key);
            }

            this.Cache.Add(key, cacheItem);
        }

        public virtual T RemoveItem(string key)
        {
            if (this.Cache.ContainsKey(key))
            {
                var value = this.Cache[key];
                this.Cache.Remove(key);

                return value.Value;
            }

            return null;
        }

        public virtual T GetItem(string key)
        {
            if (this.Cache.ContainsKey(key))
            {
                var cacheItem = this.Cache[key];

                if (cacheItem.ExpirationTime.HasValue && cacheItem.ExpirationTime < DateTime.UtcNow)
                {
                    this.RemoveItem(key);
                    return null;
                }

                return cacheItem.Value;
            }

            return null;
        }

        public virtual async Task<T> GetItem(string key, Func<Task<T>> valueFactory, TimeSpan? expiration = null)
        {
            var value = this.GetItem(key);

            if (value == null)
            {
                value = await valueFactory();
                this.AddItem(key, value, expiration);
            }

            return value;
        }
    }
}
