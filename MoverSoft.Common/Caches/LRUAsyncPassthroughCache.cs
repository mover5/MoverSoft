namespace MoverSoft.Common.Caches
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MoverSoft.Common.Definitions;

    public class LRUAsyncPassthroughCache<T> where T : class
    {
        protected class CacheRecord<TCacheValue>
        {
            public string Key { get; set; }
            public TCacheValue Value { get; set; }
            public DateTime? ExpirationTime { get; set; }
        }

        protected InsensitiveDictionary<LinkedListNode<CacheRecord<T>>> Cache { get; set; }

        protected LinkedList<CacheRecord<T>> LruList { get; set; }

        protected int KeyCapacity { get; set; }

        public LRUAsyncPassthroughCache(int keyCapacity)
        {
            this.KeyCapacity = keyCapacity;
            this.Cache = new InsensitiveDictionary<LinkedListNode<CacheRecord<T>>>();
            this.LruList = new LinkedList<CacheRecord<T>>();
        }

        public int CacheSize()
        {
            return this.Cache.Count;
        }

        public virtual void AddItem(string key, T item, TimeSpan? expiration = null)
        {
            if (this.Cache.Count >= this.KeyCapacity)
            {
                this.EvictLeastRecentlyUsedItem();
            }

            DateTime? expirationTime = expiration.HasValue ? (DateTime?)DateTime.UtcNow.Add(expiration.Value) : null;
            var cacheNode = new LinkedListNode<CacheRecord<T>>(new CacheRecord<T>
            {
                Key = key,
                Value = item,
                ExpirationTime = expirationTime
            });

            if (this.Cache.ContainsKey(key))
            {
                this.LruList.Remove(this.Cache[key]);
            }

            this.Cache[key] = cacheNode;
            this.LruList.AddLast(cacheNode);
        }

        public virtual T RemoveItem(string key)
        {
            if (this.Cache.ContainsKey(key))
            {
                var cacheNode = this.Cache[key];
                this.Cache.Remove(key);
                this.LruList.Remove(cacheNode);

                return cacheNode.Value.Value;
            }

            return null;
        }

        public virtual T GetItem(string key)
        {
            if (this.Cache.ContainsKey(key))
            {
                var cacheNode = this.Cache[key];
                this.LruList.Remove(cacheNode);
                this.LruList.AddLast(cacheNode);

                if (cacheNode.Value.ExpirationTime.HasValue && cacheNode.Value.ExpirationTime < DateTime.UtcNow)
                {
                    this.RemoveItem(key);
                    return null;
                }

                return cacheNode.Value.Value;
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

        private void EvictLeastRecentlyUsedItem()
        {
            var nodeToEvict = this.LruList.First;
            this.LruList.RemoveFirst();
            this.Cache.Remove(nodeToEvict.Value.Key);
        }
    }
}
