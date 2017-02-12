using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoverSoft.Common.Caches
{
    public class CacheItem<TValue>
    {
        public TValue Value { get; set; }

        public DateTime? Expiry { get; set; }
    }
}
