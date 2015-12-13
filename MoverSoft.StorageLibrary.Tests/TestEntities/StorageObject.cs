
namespace MoverSoft.StorageLibrary.Tests.TestEntities
{
    using MoverSoft.StorageLibrary.Entities;

    public class StorageObject : TableRecord
    {
        [Row]
        public JsonClass ClassTest { get; set; }

        [Row]
        public string TenantId { get; set; }

        [Row]
        public string ObjectId { get; set; }

        [Row]
        public string Name { get; set; }

        [Row]
        public int Count { get; set; }

        [Row]
        public StorageEnum EnumValue { get; set; }

        [Row]
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
