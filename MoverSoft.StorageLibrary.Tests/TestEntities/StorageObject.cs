
namespace MoverSoft.StorageLibrary.Tests.TestEntities
{
    using MoverSoft.StorageLibrary.Entities;

    public class StorageObject : TableRecord
    {
        [TableColumn]
        public JsonClass ClassTest { get; set; }

        [TableColumn]
        public string TenantId { get; set; }

        [TableColumn]
        public string ObjectId { get; set; }

        [TableColumn]
        public string Name { get; set; }

        [TableColumn]
        public int Count { get; set; }

        [TableColumn]
        public StorageEnum EnumValue { get; set; }

        [TableColumn]
        public string[] ArrayTest { get; set; }

        public string NotSaved { get; set; }

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
                return this.ObjectId;
            }
        }
    }
}
