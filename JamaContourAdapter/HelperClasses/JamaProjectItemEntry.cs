using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses
{
    public class JamaProjectItemEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="projectId">Project Id</param>
        /// <param name="itemId">Item Id</param>
        public JamaProjectItemEntry(int projectId, int itemId)
        {
            this.ProjectId = projectId;
            this.ItemId = itemId;
        }

        /// <summary>
        /// The id of the jama project
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The id of the jama item
        /// </summary>
        public int ItemId { get; set; }
    }
}
