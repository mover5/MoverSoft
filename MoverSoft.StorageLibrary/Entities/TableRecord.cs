
namespace MoverSoft.StorageLibrary.Entities
{
    using System;
    using MoverSoft.Common.Extensions;

    public abstract class TableRecord
    {
        public TableRecord()
        {
            this.CreatedTime = DateTime.UtcNow;
        }

        public abstract string PartitionKey { get; }

        public abstract string RowKey { get; }

        [Row]
        public DateTime CreatedTime { get; set; }

        public virtual TableRecord[] Indexes
        {
            get { return this.AsArray(); }
        }
    }
}
