using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient
{
    public class JamaItemLocation
    {
        [JsonProperty("sequence", NullValueHandling = NullValueHandling.Ignore)]
        public string Sequence { get; set; }

        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public JamaItemRef Parent { get; set; }
    }

    public class JamaItemRef
    {
        [JsonProperty("item", NullValueHandling = NullValueHandling.Ignore)]
        public int? Item { get; set; }
    }
}
