
namespace MoverSoft.StorageLibrary.Tests.TestEntities
{
    using Newtonsoft.Json;

    public class JsonClass
    {
        [JsonProperty]
        public string TestString { get; set; }

        [JsonProperty]
        public int TestInt { get; set; }

        [JsonProperty]
        public StorageEnum TestEnum { get; set; }
    }
}
