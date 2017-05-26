using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoverSoft.Common.Caches;
using System.Threading.Tasks;

namespace MoverSoft.Common.Tests
{
    [TestClass]
    public class LRUAsyncPassthroughCacheTests
    {
        [TestMethod]
        public async Task TestCache()
        {
            var cache = new LRUAsyncPassthroughCache<string>(keyCapacity: 3);

            cache.AddItem("key1", "value1");
            var result = cache.GetItem("key1");
            Assert.AreEqual("value1", result);

            result = await cache.GetItem(
                key: "key2",
                valueFactory: () =>
                {
                    return Task.FromResult("value2");
                });
            Assert.AreEqual("value2", result);

            cache.AddItem("key3", "value3");
            result = cache.GetItem("key3");
            Assert.AreEqual("value3", result);

            cache.AddItem("key4", "value4");
            result = cache.GetItem("key4");
            Assert.AreEqual("value4", result);

            // Adding key 4 should have evicted key1, since it was the Least Recently Used item
            result = cache.GetItem("key1");
            Assert.IsNull(result);

            // Adding key 5 should evict key 2
            cache.AddItem("key5", "value5");
            result = cache.GetItem("key2");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestLargeCacheMaintainsSize()
        {
            var maxCacheSize = 60;
            var cache = new LRUAsyncPassthroughCache<string>(keyCapacity: maxCacheSize);

            for (var i = 0; i < 10000; i++)
            {
                // Add logging to debug 
                var expectedSize = i < maxCacheSize ? i + 1 : maxCacheSize;
                cache.AddItem("key" + i, "value" + i);
                Assert.AreEqual(expectedSize, cache.CacheSize(), "iteration " + i);
            }
        }
    }
}
