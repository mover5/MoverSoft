using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoverSoft.Common.Utilities
{
    public class InsensitiveDictionary<TValue> : Dictionary<string, TValue>
    {
        public InsensitiveDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}
