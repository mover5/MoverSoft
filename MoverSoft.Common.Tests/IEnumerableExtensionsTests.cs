using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoverSoft.Common.Extensions;

namespace MoverSoft.Common.Tests
{
    [TestClass]
    public class IEnumerableExtensionsTests
    {
        [TestMethod]
        public void BatchEnumerableTests()
        {
            IEnumerable<int> nullEnum = null;
            var batch = nullEnum.BatchEnumerable(4);
            Assert.AreEqual(0, batch.Count());

            var partialBatch = new int[] { 1, 2, 3, 4, 5, 6 };
            batch = partialBatch.BatchEnumerable(10);
            Assert.AreEqual(1, batch.Count());
            Assert.AreEqual(6, batch.ElementAt(0).Length);

            var exactBatch = new int[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            batch = exactBatch.BatchEnumerable(8);
            Assert.AreEqual(1, batch.Count());
            Assert.AreEqual(8, batch.ElementAt(0).Length);

            var multipleFullBatches = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            batch = multipleFullBatches.BatchEnumerable(3);
            Assert.AreEqual(3, batch.Count());
            foreach (var batchPart in batch)
            {
                Assert.AreEqual(3, batchPart.Length);
            }

            var oneFullOnePartial = new int[] { 1, 2, 3, 4, 5 };
            batch = oneFullOnePartial.BatchEnumerable(3);
            Assert.AreEqual(2, batch.Count());
            Assert.AreEqual(3, batch.ElementAt(0).Length);
            Assert.AreEqual(2, batch.ElementAt(1).Length);
        }
    }
}
