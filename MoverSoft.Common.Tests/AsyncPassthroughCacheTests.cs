using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoverSoft.Common.Extensions;
using MoverSoft.Common.Caches;
using System.Threading.Tasks;

namespace MoverSoft.Common.Tests
{
    [TestClass]
    public class AsyncPassthroughCacheTests
    {
        [TestMethod]
        public async Task TestCache()
        {
            var cache = new AsyncPassthroughCache<string>();

            var result = cache.GetItem("key1");
            Assert.IsNull(result);

            cache.AddItem("key1", "value1");
            result = cache.GetItem("key1");
            Assert.AreEqual("value1", result);

            result = cache.RemoveItem("key1");
            Assert.AreEqual("value1", result);

            result = cache.GetItem("key1");
            Assert.IsNull(result);

            cache.AddItem("key1", "value1");
            result = cache.GetItem("key2");
            Assert.IsNull(result);

            var asyncMethodCalled = false;
            result = await cache.GetItem(
                key: "key2",
                valueFactory: () =>
                {
                    asyncMethodCalled = true;
                    return Task.FromResult("value2");
                });
            Assert.AreEqual("value2", result);
            Assert.IsTrue(asyncMethodCalled);

            asyncMethodCalled = false;
            result = await cache.GetItem(
                key: "key2",
                valueFactory: () =>
                {
                    asyncMethodCalled = true;
                    return Task.FromResult("value2");
                });
            Assert.AreEqual("value2", result);
            Assert.IsFalse(asyncMethodCalled);

            cache.AddItem("key3", "value3", TimeSpan.FromMilliseconds(100));
            result = cache.GetItem("key3");
            Assert.AreEqual("value3", result);

            await Task.Delay(100);

            result = cache.GetItem("key3");
            Assert.IsNull(result);
        }
    }
}
