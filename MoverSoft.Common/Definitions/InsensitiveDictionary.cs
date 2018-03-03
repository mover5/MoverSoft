namespace MoverSoft.Common.Definitions
{
    using System;
    using System.Collections.Generic;

    public class InsensitiveDictionary<TValue> : Dictionary<string, TValue>
    {
        public InsensitiveDictionary() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}
