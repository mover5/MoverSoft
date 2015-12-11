using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MoverSoft.Common.Extensions;
using MoverSoft.StorageLibrary.Entities;
using MoverSoft.StorageLibrary.Tables;
using Newtonsoft.Json;

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

        internal enum StorageEnum
        {
            Value1,
            Value2,
            Value3
        }

        internal class JsonClass
        {
            [JsonProperty]
            public string TestString { get; set; }

            [JsonProperty]
            public int TestInt { get; set; }

            [JsonProperty]
            public StorageEnum TestEnum { get; set; }
        }

        internal class StorageObject : TableRecord
        {
            [Row]
            public JsonClass ClassTest { get; set; }

            [Row]
            public string TenantId { get; set; }

            [Row]
            public string ObjectId { get; set; }

            [Row]
            public string Name { get; set; }

            [Row]
            public int Count { get; set; }

            [Row]
            public StorageEnum EnumValue { get; set; }

            [Row]
            public string[] ArrayTest { get; set; }

            public string NotSaved { get; set; }

            public override string PartitionKey
            {
                get
                {
                    return this.TenantId;
                }
            }

            public override string RowKey
            {
                get
                {
                    return this.ObjectId;
                }
            }
        }
    }
}
