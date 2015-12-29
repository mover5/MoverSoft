
namespace MoverSoft.StorageLibrary.Entities
{
    using System;
    using System.Linq;
    using MoverSoft.Common.Extensions;
    using Newtonsoft.Json;
    public abstract class TableRecord
    {
        public TableRecord()
        {
            this.CreatedTime = DateTime.UtcNow;
        }

        public TableRecord(TableRecord source)
        {
            if (source != null)
            {
                var sourceType = source.GetType();
                var thisType = this.GetType().BaseType;
                if (sourceType == thisType)
                {
                    foreach (var property in sourceType.GetProperties())
                    {
                        var attrributes = property.GetCustomAttributes(typeof(TableColumnAttribute), true);
                        if (attrributes.Any())
                        {
                            var value = property.GetValue(source, property.GetIndexParameters());
                            property.SetValue(this, value, property.GetIndexParameters());
                        }
                    }
                }
            }
        }

        [JsonIgnore]
        public abstract string PartitionKey { get; }

        [JsonIgnore]
        public abstract string RowKey { get; }

        [JsonIgnore]
        public virtual string EntityTag { get; set; }

        [TableColumn]
        [JsonIgnore]
        public DateTime CreatedTime { get; set; }

        [JsonIgnore]
        public virtual TableRecord[] Indexes
        {
            get { return this.AsArray(); }
        }
    }
}
