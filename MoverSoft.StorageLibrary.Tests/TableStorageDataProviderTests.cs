using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MoverSoft.Common.Extensions;
using MoverSoft.StorageLibrary.Tables;
using MoverSoft.StorageLibrary.Tests.TestEntities;

namespace MoverSoft.StorageLibrary.Tests
{
    [TestClass]
    public class TableStorageDataProviderTests
    {
        private string ConnectionString
        {
            get { return "UseDevelopmentStorage=true"; }
        }

        [TestMethod]
        public async Task SavesRawObjectIntoTable()
        {
            var tableName = "testtable";
            var provider = new TableStorageDataProvider(connectionString: this.ConnectionString, tableName: tableName);
            var rawStorageObject = new StorageObject
            {
                TenantId = Guid.NewGuid().ToString(),
                ObjectId = Guid.NewGuid().ToString(),
                Name = "Test User",
                Count = 10,
                EnumValue = StorageEnum.Value3,
                ClassTest = new JsonClass
                {
                    TestEnum = StorageEnum.Value2,
                    TestInt = 5,
                    TestString = "Test String"
                },
                NotSaved = "Not Saved",
                ArrayTest = new string[] { "s1", "s2" }
            };

            await provider.SaveEntity(rawStorageObject);
            var storageAccount = CloudStorageAccount.Parse(connectionString: this.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName: tableName);
            var query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, rawStorageObject.PartitionKey));
            var entities = table.ExecuteQuery(query);
            Assert.AreEqual(1, entities.Count());
            Assert.AreEqual("Test User", entities.First()["Name"].StringValue);
            Assert.AreEqual(10, entities.First()["Count"].Int32Value);
            Assert.AreEqual(rawStorageObject.ObjectId, entities.First()["ObjectId"].StringValue);
            Assert.AreEqual(rawStorageObject.TenantId, entities.First()["TenantId"].StringValue);
            Assert.AreEqual("Value3", entities.First()["EnumValue"].StringValue);
            Assert.AreEqual(rawStorageObject.ClassTest.ToJson(), entities.First()["ClassTest"].StringValue);
            Assert.IsFalse(entities.First().Properties.ContainsKey("NotSaved"));
            Assert.AreEqual(rawStorageObject.ArrayTest.ToJson(), entities.First()["ArrayTest"].StringValue);
        }

        [TestMethod]
        public async Task SaveIndexesObjectIntoTable()
        {
            var tableName = "testtableindex";
            var provider = new TableStorageDataProvider(this.ConnectionString, tableName);
            var indexedObject = new IndexedObject
            {
                Address = "Address 1",
                Name = "Name 1",
                TenantId = Guid.NewGuid().ToString()
            };

            await provider.SaveEntity(indexedObject);

            var storageAccount = CloudStorageAccount.Parse(connectionString: this.ConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(tableName: tableName);
            var query = new TableQuery().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, indexedObject.PartitionKey));
            var entities = table.ExecuteQuery(query);
            Assert.AreEqual(2, entities.Count());
            var entityIndex1 = entities.First(item => item.RowKey == IndexedObject.NameIndex.GetRowKey("Name 1"));
            var entityIndex2 = entities.First(item => item.RowKey == IndexedObject.AddressIndex.GetRowKey("Address 1", "Name 1"));
            Assert.IsNotNull(entityIndex1);
            Assert.IsNotNull(entityIndex2);
        }

        [TestMethod]
        public async Task FindEntity()
        {
            var tableName = "findentity";
            var provider = new TableStorageDataProvider(this.ConnectionString, tableName);
            var partitionKey = Guid.NewGuid().ToString();
            var entities = Enumerable.Range(0, 10)
                .SelectArray(i =>
                {
                    return new IndexedObject
                    {
                        Address = (i % 2).ToString(),
                        Name = i.ToString(),
                        TenantId = partitionKey
                    };
                });

            await provider.SaveEntities(entities);

            var name6 = await provider.Find<IndexedObject>(partitionKey, IndexedObject.NameIndex.GetRowKey("6"));
            Assert.IsNotNull(name6);
            Assert.AreEqual(partitionKey, name6.PartitionKey);
            Assert.AreEqual("0", name6.Address);
            Assert.AreEqual("6", name6.Name);

            var doesntExist = await provider.Find<IndexedObject>(partitionKey, "DoesntExist");
            Assert.IsNull(doesntExist);

            var missingPartitionKey = await provider.Find<IndexedObject>("NoPartitionKey", IndexedObject.NameIndex.GetRowKey("6"));
            Assert.IsNull(missingPartitionKey);
        }

        [TestMethod]
        public async Task FindEntitiesInPartition()
        {
            var tableName = "findentity";
            var provider = new TableStorageDataProvider(this.ConnectionString, tableName);
            var partitionKey1 = Guid.NewGuid().ToString();
            var partitionKey2 = Guid.NewGuid().ToString();
            var entities1 = Enumerable.Range(0, 10)
                .SelectArray(i =>
                {
                    return new StorageObject
                    {
                        Name = i.ToString(),
                        ObjectId = Guid.NewGuid().ToString(),
                        TenantId = partitionKey1
                    };
                });

            var entities2 = Enumerable.Range(0, 8)
                .SelectArray(i =>
                {
                    return new StorageObject
                    {
                        Name = i.ToString(),
                        ObjectId = Guid.NewGuid().ToString(),
                        TenantId = partitionKey2
                    };
                });

            await provider.SaveEntities(entities1);
            await provider.SaveEntities(entities2);

            var part1Collection = await provider.FindRange<StorageObject>(partitionKey1);
            Assert.IsNotNull(part1Collection);
            Assert.AreEqual(10, part1Collection.Count());

            var part2Collection = await provider.FindRange<StorageObject>(partitionKey2);
            Assert.IsNotNull(part2Collection);
            Assert.AreEqual(8, part2Collection.Count());

            var toppedCollection = await provider.FindRange<StorageObject>(partitionKey1, 4);
            Assert.AreEqual(4, toppedCollection.Count());

            var emptyCollection = await provider.FindRange<StorageObject>("Test");
            Assert.AreEqual(0, emptyCollection.Count());
        }

        [TestMethod]
        public async Task DeleteRecords()
        {
            var tableName = "deletetable";
            var provider = new TableStorageDataProvider(this.ConnectionString, tableName);
            var partitionKey = Guid.NewGuid().ToString();

            var entities = Enumerable.Range(0, 10)
                .SelectArray(i =>
                {
                    return new IndexedObject
                    {
                        Address = (i % 2).ToString(),
                        Name = i.ToString(),
                        TenantId = partitionKey
                    };
                });

            await provider.SaveEntities(entities);
            var ensureOriginalResult = await provider.FindRange<IndexedObject>(partitionKey);
            Assert.AreEqual(20, ensureOriginalResult.Count());

            await provider.DeleteEntity(entities[4]);
            var deletedCollection = await provider.FindRange<IndexedObject>(partitionKey);
            Assert.AreEqual(18, deletedCollection.Count());
            var deletedEntity = await provider.Find<IndexedObject>(entities[4].PartitionKey, entities[4].Indexes[0].RowKey);
            Assert.IsNull(deletedEntity);
        }

        [TestMethod]
        public async Task FindByPrefix()
        {
            var tableName = "findentity";
            var provider = new TableStorageDataProvider(this.ConnectionString, tableName);
            var partitionKey = Guid.NewGuid().ToString();
            var entities = Enumerable.Range(0, 10)
                .SelectArray(i =>
                {
                    return new IndexedObject
                    {
                        Address = (i % 2).ToString(),
                        Name = i.ToString(),
                        TenantId = partitionKey
                    };
                });

            await provider.SaveEntities(entities);

            var addressZeroPrefix = await provider.FindRange<IndexedObject>(partitionKey, IndexedObject.AddressIndex.GetRowKeyPrefix("0"));
            Assert.AreEqual(5, addressZeroPrefix.Count());

            var addressZeroPrefixWithTop = await provider.FindRange<IndexedObject>(partitionKey, IndexedObject.AddressIndex.GetRowKeyPrefix("0"), 2);
            Assert.AreEqual(2, addressZeroPrefixWithTop.Count());

            var addressZeroSegmented = await provider.FindRangeSegmented<IndexedObject>(partitionKey, IndexedObject.AddressIndex.GetRowKeyPrefix("0"), 3);
            Assert.IsNotNull(addressZeroSegmented);
            Assert.AreEqual(3, addressZeroSegmented.Results.Count());
            Assert.IsNotNull(addressZeroSegmented.ContinuationToken);

            var addressZeroNextSegment = await provider.FindRangeSegmented<IndexedObject>(partitionKey, IndexedObject.AddressIndex.GetRowKeyPrefix("0"), 3, addressZeroSegmented.ContinuationToken);
            Assert.IsNotNull(addressZeroNextSegment);
            Assert.AreEqual(2, addressZeroNextSegment.Results.Count());
            Assert.IsNull(addressZeroNextSegment.ContinuationToken);
        }

        [TestMethod]
        public async Task FindBySorted()
        {
            var tablename = "sorteddates";
            var provider = new TableStorageDataProvider(this.ConnectionString, tablename);
            var tenantId = Guid.NewGuid().ToString();
            var today = DateTime.UtcNow;

            var entities = Enumerable.Range(0, 10)
                .SelectArray(i =>
                {
                    return new DateTimeObject
                    {
                        TenantId = tenantId,
                        ObjectId = Guid.NewGuid().ToString(),
                        TheTime = today.AddDays(i)
                    };
                });

            await provider.SaveEntities(entities);

            var zeroEntities = await provider.FindRangeGreaterThanOrEqual<DateTimeObject>(tenantId, today.AddDays(12).ToSortableDateTimeString());
            Assert.AreEqual(0, zeroEntities.Count());

            var halfEntities = await provider.FindRangeGreaterThanOrEqual<DateTimeObject>(tenantId, today.AddDays(5).ToSortableDateTimeString());
            Assert.AreEqual(5, halfEntities.Count());

            zeroEntities = await provider.FindRangeGreaterThan<DateTimeObject>(tenantId, today.AddDays(9).ToSortableDateTimeString());
            Assert.AreEqual(0, zeroEntities.Count());

            halfEntities = await provider.FindRangeGreaterThan<DateTimeObject>(tenantId, today.AddDays(4).ToSortableDateTimeString());
            Assert.AreEqual(5, halfEntities.Count());

            zeroEntities = await provider.FindRangeLessThanOrEqual<DateTimeObject>(tenantId, today.AddDays(-1).ToSortableDateTimeString());
            Assert.AreEqual(0, zeroEntities.Count());

            halfEntities = await provider.FindRangeLessThanOrEqual<DateTimeObject>(tenantId, today.AddDays(4).ToSortableDateTimeString());
            Assert.AreEqual(5, halfEntities.Count());

            zeroEntities = await provider.FindRangeLessThan<DateTimeObject>(tenantId, today.AddDays(0).ToSortableDateTimeString());
            Assert.AreEqual(0, zeroEntities.Count());

            halfEntities = await provider.FindRangeLessThan<DateTimeObject>(tenantId, today.AddDays(5).ToSortableDateTimeString());
            Assert.AreEqual(5, halfEntities.Count());
        }
    }
}
