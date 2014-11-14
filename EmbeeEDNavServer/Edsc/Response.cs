using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer.Edsc
{
    internal class Response
    {
        [JsonProperty(PropertyName = "d")]
        public Data Data { get; set; }
    }

    internal class Data
    {
        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        [JsonProperty(PropertyName = "date")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime DateUpdated { get; set; }

        [JsonProperty(PropertyName = "systems")]
        public System[] Systems { get; set; }
    }

    internal class Status
    {
        [JsonProperty(PropertyName = "input")]
        public StatusInput[] StatusInputs { get; set; }
    }

    internal class StatusInput
    {
        [JsonProperty(PropertyName = "status")]
        public StatusInputStatus Status { get; set; }
    }

    internal class StatusInputStatus
    {
        [JsonProperty(PropertyName = "statusnum")]
        public int StatusNum { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Message { get; set; }
    }

    internal class System
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "coord")]
        public double[] Coordinates { get; set; }
    }


}
