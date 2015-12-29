namespace MoverSoft.StorageLibrary.Tables
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.WindowsAzure.Storage.Table;
    using MoverSoft.Common.Extensions;
    using MoverSoft.StorageLibrary.Entities;

    public static class TableRecordUtilities
    {
        public static T ConvertDynamicEntityToTableRecord<T>(this DynamicTableEntity entity) where T : TableRecord, new()
        {
            if (entity == null)
            {
                return null;
            }

            var tableRecord = new T();
            var thisType = tableRecord.GetType();
            foreach (var property in thisType.GetProperties())
            {
                var attrributes = property.GetCustomAttributes(typeof(TableColumnAttribute), true);
                if (attrributes.Any())
                {
                    EntityProperty entityProperty;
                    if (entity.Properties.TryGetValue(property.Name, out entityProperty) && entityProperty.PropertyAsObject != null)
                    {
                        var value = TableRecordUtilities.ConvertFromEntityProperty(entityProperty, property);
                        property.SetValue(tableRecord, value, property.GetIndexParameters());
                    }
                }
            }

            tableRecord.EntityTag = entity.ETag;

            return tableRecord;
        }

        public static DynamicTableEntity ConvertTableRecordToDynamicEntity(this TableRecord tableRecord)
        {
            var entity = new DynamicTableEntity(
                partitionKey: tableRecord.PartitionKey,
                rowKey: tableRecord.RowKey);

            entity.ETag = tableRecord.EntityTag ?? "*";

            var thisType = tableRecord.GetType();
            foreach (var property in thisType.GetProperties())
            {
                var attrributes = property.GetCustomAttributes(typeof(TableColumnAttribute), true);
                if (attrributes.Any())
                {
                    entity[property.Name] = TableRecordUtilities.GetEntityProperty(tableRecord, property);
                }
            }

            return entity;
        }

        public static object ConvertFromEntityProperty(EntityProperty entityProperty, PropertyInfo property)
        {
            Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (targetType == typeof(string))
            {
                return entityProperty.StringValue;
            }
            else if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, entityProperty.StringValue);
            }
            else if (targetType == typeof(DateTime))
            {
                return entityProperty.DateTimeOffsetValue.HasValue
                    ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime as DateTime?
                    : null;
            }
            else if (targetType == typeof(bool))
            {
                return entityProperty.BooleanValue;
            }
            else if (targetType == typeof(int))
            {
                return entityProperty.Int32Value;
            }
            else if (targetType == typeof(long))
            {
                return entityProperty.Int64Value;
            }
            else if (targetType == typeof(short))
            {
                return entityProperty.Int32Value.HasValue
                    ? Convert.ChangeType(entityProperty.Int32Value, typeof(short)) as short?
                    : null;
            }
            else if (targetType == typeof(double))
            {
                return entityProperty.DoubleValue;
            }
            else if (targetType == typeof(Guid))
            {
                return entityProperty.GuidValue;
            }
            else if (targetType == typeof(DateTimeOffset))
            {
                return entityProperty.DateTimeOffsetValue;
            }
            else if (targetType == typeof(byte[]))
            {
                return entityProperty.BinaryValue;
            }
            else if (entityProperty.StringValue != null)
            {
                return entityProperty.StringValue.FromJson(property.PropertyType);
            }

            throw new ArgumentException(string.Format("The property definition type '{0}' is not supported", property.PropertyType));

        }

        public static EntityProperty GetEntityProperty(TableRecord tableRecord, PropertyInfo property)
        {
            Type sourceType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            object value = property.GetValue(tableRecord, property.GetIndexParameters());
            if (sourceType == typeof(string))
            {
                return new EntityProperty(value as string);
            }
            else if (sourceType.IsEnum)
            {
                return new EntityProperty(value.ToString());
            }
            else if (sourceType == typeof(DateTime))
            {
                return new EntityProperty(value as DateTime?);
            }
            else if (sourceType == typeof(bool))
            {
                return new EntityProperty(value as bool?);
            }
            else if (sourceType == typeof(int))
            {
                return new EntityProperty(value as int?);
            }
            else if (sourceType == typeof(long))
            {
                return new EntityProperty(value as long?);
            }
            else if (sourceType == typeof(short))
            {
                return new EntityProperty(value as short?);
            }
            else if (sourceType == typeof(double))
            {
                return new EntityProperty(value as double?);
            }
            else if (sourceType == typeof(Guid))
            {
                return new EntityProperty(value as Guid?);
            }
            else if (sourceType == typeof(DateTimeOffset))
            {
                return new EntityProperty(value as DateTimeOffset?);
            }
            else if (sourceType == typeof(byte[]))
            {
                return new EntityProperty(value as byte[]);
            }

            return new EntityProperty(value.ToJson());
        }
    }
}
