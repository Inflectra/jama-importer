using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient
{
    public class JamaItem : BaseArtifact
    {
        [JsonProperty("project", NullValueHandling = NullValueHandling.Ignore)]
        public int ProjectId { get; set; }

        [JsonProperty("documentKey", NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentKey { get; set; }

        [JsonProperty("itemType", NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeId { get; set; }

        [JsonProperty("createdDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedDate { get; set; }

        [JsonProperty("modifiedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? ModifiedDate { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Fields { get; set; }

        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public JamaItemLocation Location { get; set; }

        /// <summary>
        /// The name of the item
        /// </summary>
        public string Name
        {
            get
            {
                if (this.Fields.ContainsKey("name"))
                {
                    return (string)this.Fields["name"];
                }
                return null;
            }
        }

        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description
        {
            get
            {
                if (this.Fields.ContainsKey("description"))
                {
                    return (string)this.Fields["description"];
                }
                return null;
            }
        }

        /// <summary>
        /// The parent id of the item
        /// </summary>
        public int? ParentId
        {
            get
            {
                if (this.Location != null && this.Location.Parent != null)
                {
                    return this.Location.Parent.Item;
                }
                return null;
            }
        }
    }
}
