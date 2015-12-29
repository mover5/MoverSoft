using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using MoverSoft.Common.Extensions;

namespace MoverSoft.StorageLibrary.Tables
{
    public static class TableStorageUtilities
    {
        public const string PartitionKey = "PartitionKey";

        public const string RowKey = "RowKey";

        public const int MaxTableRecords = 1000;

        public const int MaxBatchRecords = 100;

        public static string GetRowKeyQuery(string partitionKey, string rowKeyQuery)
        {
            var partitionKeyEqualsFilter = TableStorageUtilities.GetPartitionKeyEqualFilter(partitionKey);

            return TableQuery.CombineFilters(partitionKeyEqualsFilter, TableOperators.And, rowKeyQuery);
        }

        public static string GetRowKeyPrefixRangeFilter(string partitionKey, string rowKeyPrefix)
        {
            var partitionKeyEqualsFilter = TableStorageUtilities.GetPartitionKeyEqualFilter(partitionKey);
            var rowKeyPrefixFilter = TableStorageUtilities.GetRowKeyPrefixRangeFilter(rowKeyPrefix);

            return TableQuery.CombineFilters(partitionKeyEqualsFilter, TableOperators.And, rowKeyPrefixFilter);
        }

        public static string GetRowKeyPrefixRangeFilter(string rowKeyPrefix)
        {
            var left = TableQuery.GenerateFilterCondition(TableStorageUtilities.RowKey, QueryComparisons.GreaterThanOrEqual, rowKeyPrefix);
            var right = TableQuery.GenerateFilterCondition(TableStorageUtilities.RowKey, QueryComparisons.LessThan, TableStorageUtilities.GetRowKeyPrefixUpperBound(rowKeyPrefix));

            return TableQuery.CombineFilters(left, TableOperators.And, right);
        }

        public static string GetRowKeyQueryComparisonRangeFilter(string partitionKey, string rowKey, string comparison)
        {
            var partitionKeyQuery = TableStorageUtilities.GetPartitionKeyEqualFilter(partitionKey);
            var rowKeyQuery = TableQuery.GenerateFilterCondition(TableStorageUtilities.RowKey, comparison, rowKey);

            return TableQuery.CombineFilters(partitionKeyQuery, TableOperators.And, rowKeyQuery);
        }

        public static string GetRowKeyRangeFilter(string rowKeyStart, string rowKeyEnd)
        {
            var left = TableQuery.GenerateFilterCondition(TableStorageUtilities.RowKey, QueryComparisons.GreaterThanOrEqual, rowKeyStart);
            var right = TableQuery.GenerateFilterCondition(TableStorageUtilities.RowKey, QueryComparisons.LessThan, rowKeyEnd);

            return TableQuery.CombineFilters(left, TableOperators.And, right);
        }

        public static string GetRowKeyEqualFilter(string rowKey)
        {
            return TableQuery.GenerateFilterCondition(TableStorageUtilities.RowKey, QueryComparisons.Equal, rowKey);
        }

        public static string GetPartitionKeyEqualFilter(string partitionKey)
        {
            return TableQuery.GenerateFilterCondition(TableStorageUtilities.PartitionKey, QueryComparisons.Equal, partitionKey);
        }

        public static string GetRowKeyPrefixUpperBound(string rowKeyPrefix)
        {
            var sb = new StringBuilder(rowKeyPrefix);
            sb[sb.Length - 1]++;

            return sb.ToString();
        }

        public static string CombineStorageKeys(params string[] keys)
        {
            if (keys.Any(key => key.Contains('-')))
            {
                var invalidKey = keys.First(key => key.Contains('-'));
                throw new ArgumentException(string.Format("The storage key '{0}' is not properly encoded.", invalidKey), "keys");
            }

            return keys.ConcatStrings("-");
        }

        public static string EscapeStorageKey(string storageKey)
        {
            StringBuilder escapedStorageKey = new StringBuilder(storageKey.Length);
            foreach (char c in storageKey)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    escapedStorageKey.Append(TableStorageUtilities.EscapeStorageCharacter(c));
                }
                else
                {
                    escapedStorageKey.Append(c);
                }
            }

            return escapedStorageKey.ToString();
        }

        public static string EscapeGuidStorageKey(string storageKey)
        {
            return TableStorageUtilities.EscapeStorageKey(storageKey.Replace("-", string.Empty).ToUpperInvariant());
        }

        private static string EscapeStorageCharacter(char character)
        {
            var ordinalValue = (ushort)character;
            if (ordinalValue < 0x100)
            {
                return string.Format(CultureInfo.InvariantCulture, ":{0:X2}", ordinalValue);
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "::{0:X4}", ordinalValue);
            }
        }
    }
}
