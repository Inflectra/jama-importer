using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.DataSets;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses
{
    /// <summary>
    /// Manages the mapping of release data between SpiraTeam and Jama Contour
    /// </summary>
    /// <remarks>Uses an XML-serialized Dataset to store the data</remarks>
    public class ReleaseDataMapping
    {
        public const string DATA_FOLDER = "JamaContourAdapter";
        public const string DATA_FILE = "ReleaseDataMapping.xml";

        private ReleaseMappingData releaseMappingData;
        private string mappingDataFile = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public ReleaseDataMapping()
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
            this.releaseMappingData = new ReleaseMappingData();
            if (File.Exists(mappingDataFile))
            {
                //Read in the data
                releaseMappingData.ReadXml(mappingDataFile, System.Data.XmlReadMode.Auto);
            }
            else
            {
                //Write out the empty data set
                releaseMappingData.WriteXml(mappingDataFile);
            }
        }

        /// <summary>
        /// Gets a data mapping row from the corresponding Jama project id and item id
        /// </summary>
        /// <param name="jamaProjectId">The id of the Jama project</param>
        /// <param name="jamaReleaseId">The id of the Jama release</param>
        /// <returns>The matching datarow (or null if no match)</returns>
        public ReleaseMappingData.ReleaseMappingRow GetFromJamaId(int jamaProjectId, int jamaReleaseId)
        {
            if (this.releaseMappingData == null)
            {
                return null;
            }
            return this.releaseMappingData.ReleaseMapping.FindByJamaProjectIdJamaReleaseId(jamaProjectId, jamaReleaseId);
        }

        /// <summary>
        /// Gets a data mapping row from the corresponding Spira project id and release id
        /// </summary>
        /// <param name="spiraProjectId">The id of the Spira project</param>
        /// <param name="spiraReleaseId">The id of the Spira release</param>
        /// <returns>The matching datarow (or null if no match)</returns>
        public ReleaseMappingData.ReleaseMappingRow GetFromSpiraId(int spiraProjectId, int spiraReleaseId)
        {
            if (this.releaseMappingData == null)
            {
                return null;
            }

            //There is no primary key, so just iterate and return first match
            foreach (ReleaseMappingData.ReleaseMappingRow dataRow in this.releaseMappingData.ReleaseMapping)
            {
                if (dataRow.SpiraProjectId == spiraProjectId && dataRow.SpiraReleaseId == spiraReleaseId)
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
        /// <param name="spiraReleaseId">The id of the spira release</param>
        /// <param name="jamaProjectId">The id of the jama project</param>
        /// <param name="jamaReleaseId">The id of the jama release</param>
        public void Add(int spiraProjectId, int spiraReleaseId, int jamaProjectId, int jamaReleaseId)
        {
            //Create a new data row
            if (this.releaseMappingData != null)
            {
                ReleaseMappingData.ReleaseMappingRow dataRow = this.releaseMappingData.ReleaseMapping.AddReleaseMappingRow(
                    spiraProjectId,
                    spiraReleaseId,
                    jamaProjectId,
                    jamaReleaseId
                    );
                dataRow.AcceptChanges();
            }
        }

        /// <summary>
        /// Clears all the mapping data from memory (doesn't save the change)
        /// </summary>
        public void Clear()
        {
            if (this.releaseMappingData != null)
            {
                this.releaseMappingData.ReleaseMapping.Clear();
            }
        }

        /// <summary>
        /// Saves the current mapping data to the XML file
        /// </summary>
        public void Save()
        {
            //Write out the empty data set
            if (this.releaseMappingData != null && !String.IsNullOrEmpty(mappingDataFile))
            {
                releaseMappingData.WriteXml(mappingDataFile);
            }
        }

        public void Delete(int jamaProjectId, int jamaReleaseId)
        {
            ReleaseMappingData.ReleaseMappingRow dataRow = GetFromJamaId(jamaProjectId, jamaReleaseId);
            if (dataRow != null)
            {
                dataRow.Delete();
                dataRow.AcceptChanges();
            }
        }
    }
}
