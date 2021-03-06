﻿
namespace MoverSoft.StorageLibrary.Tests.TestEntities
{
    using System;
    using MoverSoft.Common.Extensions;
    using MoverSoft.StorageLibrary.Entities;
    using Tables;

    public class DateTimeObject : TableRecord
    {
        [TableColumn]
        public string TenantId { get; set; }

        [TableColumn]
        public string ObjectId { get; set; }

        [TableColumn]
        public DateTime TheTime { get; set; }

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
                return TableStorageUtilities.EscapeStorageKey(this.TheTime.ToSortableDateTimeString()); 
            }
        }
    }
}
