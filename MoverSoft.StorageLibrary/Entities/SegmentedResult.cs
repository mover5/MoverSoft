
namespace MoverSoft.StorageLibrary.Entities
{
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage.Table;

    public class SegmentedResult<T>
    {
        public TableContinuationToken ContinuationToken { get; set; }

        public T[] Results { get; set; }
    }
}
