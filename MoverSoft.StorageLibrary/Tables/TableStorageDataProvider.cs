namespace MoverSoft.StorageLibrary.Tables
{
    using System;
    using System.Collections.Generic;
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

        public async Task<T[]> FindRange<T>(string partitionKey, int? top = null) where T : TableRecord, new()
        {
            var segmentedResult = await this
                .FindRangeSegmented<T>(partitionKey: partitionKey, top: top)
                .ConfigureAwait(continueOnCapturedContext: false);

            return segmentedResult.Results;
        }

        public async Task<T[]> FindRange<T>(string partitionKey, string rowKeyPrefix, int? top = null) where T : TableRecord, new()
        {
            var segmentedResult = await this
                .FindRangeSegmented<T>(partitionKey: partitionKey, rowKeyPrefix: rowKeyPrefix, top: top)
                .ConfigureAwait(continueOnCapturedContext: false);

            return segmentedResult.Results;
        }

        public Task<SegmentedResult<T>> FindRangeSegmented<T>(string partitionKey, int? top = null, TableContinuationToken token = null) where T : TableRecord, new()
        {
            var partitionKeyQuery = TableStorageUtilities.GetPartitionKeyEqualFilter(partitionKey: partitionKey);

            return this.FindRangeSegmentedInternal<T>(partitionKeyQuery, top, token);
        }

        public Task<SegmentedResult<T>> FindRangeSegmented<T>(string partitionKey, string rowKeyPrefix, int? top = null, TableContinuationToken token = null) where T : TableRecord, new()
        {
            var rowPrefixQuery = TableStorageUtilities.GetRowKeyPrefixRangeFilter(partitionKey, rowKeyPrefix);

            return this.FindRangeSegmentedInternal<T>(rowPrefixQuery, top, token);
        }

        private async Task<SegmentedResult<T>> FindRangeSegmentedInternal<T>(string query, int? top = null, TableContinuationToken token = null) where T : TableRecord, new()
        {
            top = top.HasValue ? (int?)Math.Min(top.Value, TableStorageUtilities.MaxTableRecords) : null;

            var entitySegment = await this.Table
                .ExecuteQuerySegmentedAsync(
                    query: new TableQuery<DynamicTableEntity>().Where(query).Take(top),
                    token: token)
                .ConfigureAwait(continueOnCapturedContext: false);

            return new SegmentedResult<T>
            {
                ContinuationToken = entitySegment.ContinuationToken,
                Results = entitySegment.Results
                    .CoalesceEnumerable()
                    .SelectArray(entity => entity.ConvertDynamicEntityToTableRecord<T>())
            };
        }

        public Task DeleteEntity(TableRecord record)
        {
            return this
                .SaveAndDeleteEntities(
                    toSave: null,
                    toDelete: record.AsArray());
        }

        public Task DeleteEntities(IEnumerable<TableRecord> records)
        {
            return this
                .SaveAndDeleteEntities(
                    toSave: null,
                    toDelete: records);
        }

        public Task SaveEntity(TableRecord record)
        {
            return this
                .SaveAndDeleteEntities(
                    toSave: record.AsArray(),
                    toDelete: null);
        }

        public Task SaveEntities(IEnumerable<TableRecord> records)
        {
            return this
                .SaveAndDeleteEntities(
                    toSave: records,
                    toDelete: null);
        }

        public async Task SaveAndDeleteEntities(IEnumerable<TableRecord> toSave, IEnumerable<TableRecord> toDelete)
        {
            var batchOperation = new TableBatchOperation();
            var indexesToSave = toSave
                .CoalesceEnumerable()
                .SelectManyArray(value => value.Indexes);

            var indexesToDelete = toDelete
                .CoalesceEnumerable()
                .SelectManyArray(value => value.Indexes);

            var allIndexes = indexesToSave.Concat(indexesToDelete);
            if (allIndexes.Count() > TableStorageUtilities.MaxBatchRecords)
            {
                throw new ArgumentException(string.Format("Too many batch indexes. Index count: {0}. Max Indexes: {1}", allIndexes.Count(), TableStorageUtilities.MaxBatchRecords));
            }

            var partitionKeys = allIndexes.Select(record => record.PartitionKey).DistinctArray();
            if (partitionKeys.Count() != 1)
            {
                throw new ArgumentException("All entities must belong to the same partition key");
            }

            foreach (var indexToSave in indexesToSave)
            {
                if (string.IsNullOrEmpty(indexToSave.PartitionKey))
                {
                    throw new ArgumentException("Partition key must not be null or empty");
                }

                if (string.IsNullOrEmpty(indexToSave.RowKey))
                {
                    throw new ArgumentException("Row key must not be null or empty");
                }

                batchOperation.InsertOrReplace(indexToSave.ConvertTableRecordToDynamicEntity());
            }

            foreach (var indexToDelete in indexesToDelete)
            {
                if (string.IsNullOrEmpty(indexToDelete.PartitionKey))
                {
                    throw new ArgumentException("Partition key must not be null or empty");
                }

                if (string.IsNullOrEmpty(indexToDelete.RowKey))
                {
                    throw new ArgumentException("Row key must not be null or empty");
                }

                batchOperation.Delete(indexToDelete.ConvertTableRecordToDynamicEntity());
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
