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

        public async Task<T> Find<T>(string partitionKey, string rowKey) where T : TableRecord, new()
        {
            var result = await this.Table
                .ExecuteAsync(TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey))
                .ConfigureAwait(continueOnCapturedContext: false);

            return ((DynamicTableEntity)result.Result).ConvertDynamicEntityToTableRecord<T>();
        }

        public async Task<T[]> FindRange<T>(string partitionKey) where T : TableRecord, new()
        {
            var segmentedResult = await this
                .FindRangeSegmented<T>(partitionKey: partitionKey)
                .ConfigureAwait(continueOnCapturedContext: false);

            return segmentedResult.Results;
        }

        public async Task<SegmentedResult<T>> FindRangeSegmented<T>(string partitionKey, TableContinuationToken token = null) where T : TableRecord, new()
        {
            var rangeQuery = new TableQuery()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var entitySegment = await this.Table
                .ExecuteQuerySegmentedAsync(rangeQuery, token)
                .ConfigureAwait(continueOnCapturedContext: false);

            return new SegmentedResult<T>
            {
                ContinuationToken = entitySegment.ContinuationToken,
                Results = entitySegment.Results != null
                    ? entitySegment.Results.SelectArray(entity => entity.ConvertDynamicEntityToTableRecord<T>())
                    : null
            };
        }

        public Task DeleteEntity<T>(T record) where T : TableRecord, new()
        {
            return Table.ExecuteAsync(TableOperation.Delete(record.ConvertTableRecordToDynamicEntity()));
        }

        public async Task SaveEntity<T>(T record) where T : TableRecord, new()
        {
            var batchOperation = new TableBatchOperation();
            var indexes = record.Indexes;
            var partitionKeys = indexes
                .CoalesceEnumerable()
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
                await this.Table
                    .ExecuteBatchAsync(batch: batchOperation)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
        }
    }
}
