using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeeEDNavServer.Edsc
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RequestFilter
    {
        /// <summary>
        /// knownstatus filters on if a systems coordinates are known or not
        /// 0 (All) Default - Returns all systems, ie. "no filter".
        /// 1 (Known) - Returns only systems with known coordinates.
        /// 2 (Unknown) - Returns only systems not having known coordinates.
        /// </summary>
        [JsonProperty(PropertyName = "knownstatus")]
        public int KnownStatus { get; set; }

        /// <summary>
        /// cr Confidence Rating. How many times have an entry been confirmed. 
        //  With a cr of 1 it could be a mis-typed system name. The chance of which is reduced with a higher cr number. Records with a cr equal or higher than the supplied value are returned.
        /// </summary>
        [JsonProperty(PropertyName = "cr")]
        public int ConfidenceRating { get; set; }

        /// <summary>
        /// Date of last update.
        /// Records with an update date equal or higher than date are returned.
        /// Suggested use is to keep note of your last pull, and then only request records newer than that timestamp.
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime LastUpdate { get; set; }

        // there are other filters, but we only care about these 3 for now

    }
}
