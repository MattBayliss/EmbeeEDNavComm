using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer.Edsc
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DataWrapper
    {
        [JsonProperty(PropertyName = "data")]
        public GetSystemsRequest Data { get; set; }
    }
}
