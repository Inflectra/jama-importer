using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient
{
    public class JamaProject : BaseArtifact
    {
        [JsonProperty("projectKey", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectKey { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public JamaFields Fields { get; set; }
    }
}
