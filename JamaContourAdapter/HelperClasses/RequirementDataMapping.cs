using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.DataSets;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses
{
    /// <summary>
    /// Manages the mapping of requirement data between SpiraTeam and Jama Contour
    /// </summary>
    /// <remarks>Uses an XML-serialized Dataset to store the data</remarks>
    public class RequirementDataMapping
    {
        public const string DATA_FOLDER = "JamaContourAdapter";
        public const string DATA_FILE = "RequirementDataMapping.xml";

        private RequirementMappingData requirementMappingData;
        private string mappingDataFile = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public RequirementDataMapping()
        {
            //First we need to get the application roaming settings folder
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Inflectra\" + DATA_FOLDER;

            //If it doesn't exist, create it
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            //Now see if the DataSet xml exists
            this.mappingDataFile = appDataFolder + @"\" + DATA_FILE;
            this.requirementMappingData = new RequirementMappingData();
            if (File.Exists(mappingDataFile))
            {
                //Read in the data
                requirementMappingData.ReadXml(mappingDataFile, System.Data.XmlReadMode.Auto);
            }
            else
            {
                //Write out the empty data set
                requirementMappingData.WriteXml(mappingDataFile);
            }
        }

        /// <summary>
        /// Gets a data mapping row from the corresponding Jama project id and item id
        /// </summary>
        /// <param name="jamaProjectId">The id of the Jama project</param>
        /// <param name="jamaItemId">The id of the Jama item</param>
        /// <returns>The matching datarow (or null if no match)</returns>
        public RequirementMappingData.RequirementMappingRow GetFromJamaId(int jamaProjectId, int jamaItemId)
        {
            if (this.requirementMappingData == null)
            {
                return null;
            }
            return this.requirementMappingData.RequirementMapping.FindByJamaProjectIdJamaItemId(jamaProjectId, jamaItemId);
        }

        /// <summary>
        /// Gets a data mapping row from the corresponding Spira project id and requirement id
        /// </summary>
        /// <param name="spiraProjectId">The id of the Spira project</param>
        /// <param name="spiraReqId">The id of the Spira requirement</param>
        /// <returns>The matching datarow (or null if no match)</returns>
        public RequirementMappingData.RequirementMappingRow GetFromSpiraId(int spiraProjectId, int spiraReqId)
        {
            if (this.requirementMappingData == null)
            {
                return null;
            }

            //There is no primary key, so just iterate and return first match
            foreach (RequirementMappingData.RequirementMappingRow dataRow in this.requirementMappingData.RequirementMapping)
            {
                if (dataRow.SpiraProjectId == spiraProjectId && dataRow.SpiraRequirementId == spiraReqId)
                {
                    return dataRow;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a new mapping entry
        /// </summary>
        /// <param name="spiraProjectId">The id of the spira project</param>
        /// <param name="spiraReqId">The id of the spira requirement</param>
        /// <param name="jamaProjectId">The id of the jama project</param>
        /// <param name="jamaItemId">The id of the jama item</param>
        public void Add(int spiraProjectId, int spiraReqId, int jamaProjectId, int jamaItemId)
        {
            //Create a new data row
            if (this.requirementMappingData != null)
            {
                RequirementMappingData.RequirementMappingRow dataRow = this.requirementMappingData.RequirementMapping.AddRequirementMappingRow(
                    spiraProjectId,
                    spiraReqId,
                    jamaProjectId,
                    jamaItemId
                    );
                dataRow.AcceptChanges();
            }
        }

        /// <summary>
        /// Clears all the mapping data from memory (doesn't save the change)
        /// </summary>
        public void Clear()
        {
            if (this.requirementMappingData != null)
            {
                this.requirementMappingData.RequirementMapping.Clear();
            }
        }

        /// <summary>
        /// Saves the current mapping data to the XML file
        /// </summary>
        public void Save()
        {
            //Write out the empty data set
            if (this.requirementMappingData != null && !String.IsNullOrEmpty(mappingDataFile))
            {
                requirementMappingData.WriteXml(mappingDataFile);
            }
        }

        /// <summary>
        /// Returns the list of requirements mapping rows
        /// </summary>
        public RequirementMappingData.RequirementMappingDataTable Rows
        {
            get
            {
                return this.requirementMappingData.RequirementMapping;
            }
        }

        public void Delete(int jamaProjectId, int jamaItemId)
        {
            RequirementMappingData.RequirementMappingRow dataRow = GetFromJamaId(jamaProjectId, jamaItemId);
            if (dataRow != null)
            {
                dataRow.Delete();
                dataRow.AcceptChanges();
            }
        }
    }
}
