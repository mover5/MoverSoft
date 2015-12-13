
namespace MoverSoft.StorageLibrary.Entities
{
    using System;
    using System.Linq;
    using MoverSoft.Common.Extensions;

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
                        var attrributes = property.GetCustomAttributes(typeof(RowAttribute), true);
                        if (attrributes.Any())
                        {
                            var value = property.GetValue(source, property.GetIndexParameters());
                            property.SetValue(this, value, property.GetIndexParameters());
                        }
                    }
                }
            }
        }

        public abstract string PartitionKey { get; }

        public abstract string RowKey { get; }

        public virtual string EntityTag { get; set; }

        [Row]
        public DateTime CreatedTime { get; set; }

        public virtual TableRecord[] Indexes
        {
            get { return this.AsArray(); }
        }
    }
}
