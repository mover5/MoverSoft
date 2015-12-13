namespace MoverSoft.StorageLibrary.Tests.TestEntities
{
    using System;
    using MoverSoft.StorageLibrary.Entities;
    using MoverSoft.StorageLibrary.Tables;

    public class IndexedObject : TableRecord
    {
        public IndexedObject() : base() { }

        public IndexedObject(IndexedObject source) : base(source) { }

        [Row]
        public string TenantId { get; set; }

        [Row]
        public string Name { get; set; }

        [Row]
        public string Address { get; set; }

        public override string PartitionKey
        {
            get { return this.TenantId; }
        }

        public override string RowKey
        {
            get { throw new Exception("Use index"); }
        }

        public override TableRecord[] Indexes
        {
            get
            {
                return new TableRecord[]
                {
                        new NameIndex(this),
                        new AddressIndex(this)
                };
            }
        }

        public class NameIndex : IndexedObject
        {
            private const string IndexId = "INI";

            public NameIndex() : base() { }

            public NameIndex(IndexedObject source) : base(source) { }

            public override string RowKey
            {
                get { return NameIndex.GetRowKey(this.Name); }
            }

            public static string GetRowKey(string name)
            {
                return TableStorageUtilities.CombineStorageKeys(
                    NameIndex.IndexId,
                    TableStorageUtilities.EscapeStorageKey(name.ToUpper()));
            }

            public static string GetRowKeyPrefix()
            {
                return TableStorageUtilities.CombineStorageKeys(
                    NameIndex.IndexId,
                    string.Empty);
            }
        }

        public class AddressIndex : IndexedObject
        {
            private const string IndexId = "IAI";

            public AddressIndex() : base() { }

            public AddressIndex(IndexedObject source) : base(source) { }

            public override string RowKey
            {
                get { return AddressIndex.GetRowKey(this.Address, this.Name); }
            }

            public static string GetRowKey(string address, string name)
            {
                return TableStorageUtilities.CombineStorageKeys(
                    AddressIndex.IndexId,
                    TableStorageUtilities.EscapeStorageKey(address.ToUpper()),
                    TableStorageUtilities.EscapeStorageKey(name.ToUpper()));
            }

            public static string GetRowKeyPrefix(string address)
            {
                return TableStorageUtilities.CombineStorageKeys(
                    AddressIndex.IndexId,
                    TableStorageUtilities.EscapeStorageKey(address.ToUpper()),
                    string.Empty);
            }
        }
    }
}
