using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoverSoft.Common.Caches;

namespace MoverSoft.Common.Tests
{
    [TestClass]
    public class AsyncPassThroughCacheTests
    {
        [TestMethod]
        public void BasicGetSet()
        {
            var cache = new AsyncPassThroughCache<string>();
            Assert.IsNull(cache.GetValue("testKey1"));
            cache.SetValue("testKey1", "testValue1");
            Assert.AreEqual("testValue1", cache.GetValue("testKey1"));
            Assert.AreEqual("testValue1", cache.GetValue("testKEY1"));
        }

        [TestMethod]
        public void PopValues()
        {
            var cache = new AsyncPassThroughCache<string>();
            cache.SetValue("testKey1", "value1");
            cache.SetValue("testKey2", "value2");
            Assert.AreEqual("value2", cache.PopValue("testKey2"));
            Assert.IsNull(cache.GetValue("testKey2"));
            Assert.IsNull(cache.GetValue("testKey3"));
        }

        [TestMethod]
        public void PassthroughGetValue()
        {
            var cache = new AsyncPassThroughCache<string>();

            var factoryCalled = false;
            var value = cache.GetValue(
                cacheKey: "key1",
                valueFactory: () =>
                {
                    factoryCalled = true;
                    return "value1";
                });

            Assert.AreEqual("value1", value);
            Assert.IsTrue(factoryCalled);

            factoryCalled = false;
            value = cache.GetValue(
                cacheKey: "key1",
                valueFactory: () =>
                {
                    factoryCalled = true;
                    return "value1";
                });

            Assert.AreEqual("value1", value);
            Assert.IsFalse(factoryCalled);
        }

        [TestMethod]
        public async Task PassThroughValueExpiry()
        {
            var cache = new AsyncPassThroughCache<string>();

            var value = cache.GetValue(
                cacheKey: "key1",
                cacheItemExpiry: TimeSpan.FromSeconds(2),
                valueFactory: () => "value1");

            Assert.AreEqual("value1", value);
            Assert.AreEqual("value1", cache.GetValue("key1"));

            await Task.Delay(TimeSpan.FromSeconds(2));

            Assert.IsNull(cache.GetValue("key1"));
        }
    }
}
