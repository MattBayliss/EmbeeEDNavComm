using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer.Edsc
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GetSystemsRequest
    {
        [JsonProperty(PropertyName="ver")]
        public int Version { get { return 2; } }

        [JsonProperty(PropertyName = "test")]
        public bool UseTestDb
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }


        [JsonProperty(PropertyName = "outputmode")]
        public int OutputMode { get; set; }

        [JsonProperty(PropertyName = "filter")]
        public RequestFilter Filter { get; set; }

    }
}
