namespace MoverSoft.StorageLibrary.Tables
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Table;
    using MoverSoft.Common.Extensions;
    using MoverSoft.StorageLibrary.Entities;

    public class TableStorageDataProvider
    {
        private CloudStorageAccount StorageAccount { get; set; }

        private CloudTableClient TableClient { get; set; }

        private CloudTable Table { get; set; }

        public TableStorageDataProvider(string connectionString, string tableName)
        {
            this.StorageAccount = CloudStorageAccount.Parse(connectionString: connectionString);
            this.TableClient = this.StorageAccount.CreateCloudTableClient();
            this.Table = this.TableClient.GetTableReference(tableName: tableName);
            this.Table.CreateIfNotExists(requestOptions: new TableRequestOptions
            {
                LocationMode = LocationMode.PrimaryThenSecondary
            });
        }

        public async Task<T> FindEntity<T>(string partitionKey, string rowKey) where T : TableRecord, new()
        {
            var result = await this.Table.ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey));
            return ((DynamicTableEntity)result.Result).ConvertDynamicEntityToTableRecord<T>();
        }

        public async Task SaveEntity<T>(T record) where T : TableRecord, new()
        {
            var batchOperation = new TableBatchOperation();
            var indexes = record.Indexes;
            var partitionKeys = indexes
                .SelectArray(index => index.PartitionKey)
                .Distinct();

            if (partitionKeys.Count() != 1)
            {
                throw new ArgumentException("All index partition keys must match");
            }

            foreach (var index in indexes)
            {
                if (string.IsNullOrEmpty(index.PartitionKey))
                {
                    throw new ArgumentException("Partition key must not be null or empty");
                }

                if (string.IsNullOrEmpty(index.RowKey))
                {
                    throw new ArgumentException("Row key must not be null or empty");
                }

                batchOperation.InsertOrReplace(index.ConvertTableRecordToDynamicEntity());
            }

            if (batchOperation.Any())
            {
                await this.Table.ExecuteBatchAsync(batch: batchOperation);
            }
        }
    }
}
