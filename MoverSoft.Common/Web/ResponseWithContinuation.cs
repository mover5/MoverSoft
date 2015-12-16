using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MoverSoft.Common.Web
{
    public class ResponseWithContinuation<T>
    {
        [JsonProperty]
        public T[] Value { get; set; }

        [JsonProperty]
        public string NextLink { get; set; }
    }
}
