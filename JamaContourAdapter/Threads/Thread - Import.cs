using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI;
using System.Windows.Threading;
using System.Threading;
using System.Linq;
using System.IO;

using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.DataSets;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.SpiraSoapService;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	internal partial class ProcessThread
	{
        private const int REQUEST_PAGE_SIZE = 25;

        private RequirementDataMapping _requirementDataMapping;
        private ReleaseDataMapping _releaseDataMapping;
        private string _projectShortName = "";

		/// <summary>The main processing function.</summary>
		/// <param name="ErrorMsg">Error message in case an error happens.</param>
		/// <returns>Boolean on status of import.</returns>
        private bool ProcessImport(StreamWriter streamWriter, out FinalStatusEnum Status)
		{
			Status = FinalStatusEnum.OK;
            this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = 2 });
            streamWriter.WriteLine("Starting Import...");

            try
            {
                //Load the mapping data
                this._requirementDataMapping = new RequirementDataMapping();
                this._releaseDataMapping = new ReleaseDataMapping();

                //See if we've been asked to clear the saved mapping data
                if (this._SpiraProject.RootReq == -1)
                {
                    this._requirementDataMapping.Clear();
                    this._releaseDataMapping.Clear();
                }
                else
                {
                    //Get the list of Releases in the Jama project
                    JamaProject jamaProject = this._jamaClient.GetProject(this._JamaProject.ProjectNum);
                    if (jamaProject != null)
                    {
                        //Store the project short name
                        this._projectShortName = jamaProject.ProjectKey;

                        List<JamaRelease> jamaReleases = this._jamaClient.GetReleases(jamaProject.Id);

                        //Import the releases into SpiraTeam
                        if (jamaReleases != null)
                        {
                            foreach (JamaRelease jamaRelease in jamaReleases)
                            {
                                if (ProcessThread.WantCancel)
                                {
                                    break;
                                }
                                FinalStatusEnum thisRun = this.ProcessRelease(streamWriter, jamaRelease);
                                if (Status != FinalStatusEnum.Error && thisRun == FinalStatusEnum.Error)
                                {
                                    Status = thisRun;
                                }
                                if (Status == FinalStatusEnum.OK && thisRun == FinalStatusEnum.Warning)
                                {
                                    Status = thisRun;
                                }
                            }
                        }
                    }

                    //Get the list of items in the Jama project in batches of 25
                    int startItem = 1;
                    List<JamaItem> jamaItems = null;

                    //Store the list of jama project ids and item ids so that we can track deletes later
                    List<JamaProjectItemEntry> jamaEntries = new List<JamaProjectItemEntry>();

                    do
                    {
                        jamaItems = this._jamaClient.GetItemsForProject(this._JamaProject.ProjectNum, startItem, REQUEST_PAGE_SIZE);

                        //Import the items into SpiraTeam
                        if (jamaItems != null)
                        {
                            foreach (JamaItem jamaItem in jamaItems)
                            {
                                if (ProcessThread.WantCancel)
                                {
                                    break;
                                }
                                FinalStatusEnum thisRun = this.ProcessItem(streamWriter, jamaItem);
                                if (Status != FinalStatusEnum.Error && thisRun == FinalStatusEnum.Error)
                                {
                                    Status = thisRun;
                                }
                                if (Status == FinalStatusEnum.OK && thisRun == FinalStatusEnum.Warning)
                                {
                                    Status = thisRun;
                                }

                                //Also add to the list so that we can check for deletes afterwards
                                jamaEntries.Add(new JamaProjectItemEntry(jamaItem.ProjectId, jamaItem.Id));
                            }
                        }

                        startItem += REQUEST_PAGE_SIZE;
                    }
                    while (jamaItems != null && jamaItems.Count > 0 && !ProcessThread.WantCancel);

                    //See if we have at least v3.1 of SpiraTest, since only that supports deletes
                    RemoteVersion remoteVersion = this._spiraClient.System_GetProductVersion();
                    if (remoteVersion != null)
                    {
                        string versionNumber = remoteVersion.Version;
                        string[] versionComponents = versionNumber.Split('.');
                        int majorVersion = 0;
                        if (!Int32.TryParse(versionComponents[0], out majorVersion))
                        {
                            majorVersion = 0;
                        }
                        int minorVersion = 0;
                        if (versionComponents.Length > 1)
                        {
                            if (!Int32.TryParse(versionComponents[1], out minorVersion))
                            {
                                minorVersion = 0;
                            }
                        }

                        //We need at least v3.1
                        if (majorVersion > 3 || (majorVersion == 3 && minorVersion >= 1))
                        {
                            //Now see if we have any mapped requirements that are no longer in Jama
                            if (this._requirementDataMapping.Rows != null)
                            {
                                for (int i = 0; i < this._requirementDataMapping.Rows.Count; i++)
                                {
                                    RequirementMappingData.RequirementMappingRow mappingRow = this._requirementDataMapping.Rows[i];
                                    //See that entry exists in the list of items, if not, delete from Spira and mappings
                                    if (!jamaEntries.Any(je => je.ItemId == mappingRow.JamaItemId && je.ProjectId == mappingRow.JamaProjectId))
                                    {
                                        DeleteSpiraRequirement(streamWriter, mappingRow);
                                    }
                                }
                            }
                        }
                    }
                }

                //Save the mapping data
                this._requirementDataMapping.Save();
                this._releaseDataMapping.Save();

                if (Status == FinalStatusEnum.Error)
                {
                    string ErrorMsg = "";
                    if (ProcessThread.WantCancel)
                    {
                        ErrorMsg = App.CANCELSTRING;
                    }
                    this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 2 });
                    return false;
                }
                else
                {
                    this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = 2 });
                    return true;
                }
            }
            catch (Exception exception)
            {
                //Handle all exceptions
                streamWriter.WriteLine("Error: " + exception.Message);
                this.ProgressUpdate(this, new ProgressArgs() { ErrorText = exception.Message, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 2 });
                return false;
            }
		}

        /// <summary>
        /// Deletes a specific spira requirement and its associated mapping row
        /// </summary>
        /// <param name="mappingRow">The mapping row to be deleted</param>
        private void DeleteSpiraRequirement(StreamWriter streamWriter, RequirementMappingData.RequirementMappingRow mappingRow)
        {
            //First delete the item in SpiraTeam
            try
            {
                this._spiraClient.Requirement_Delete(mappingRow.SpiraRequirementId);

                //Finally delete the mapping row
                mappingRow.Delete();
            }
            catch (Exception exception)
            {
                //Log error but continue
                streamWriter.WriteLine(String.Format("Unable to delete requirement RQ{0} in SpiraTeam.", mappingRow.SpiraRequirementId) + ": " + exception.Message); 
            }
        }

        /// <summary>
        /// Processes a Jama release, importing into SpiraTeam if necessary
        /// </summary>
        /// <param name="jamaRelease">The jama release</param>
        /// <returns></returns>
        private FinalStatusEnum ProcessRelease(StreamWriter streamWriter, JamaRelease jamaRelease)
        {
            FinalStatusEnum retStatus = FinalStatusEnum.OK;

            try
            {                
                //Insert/update the release in SpiraTeam
                
                //See if the item is already mapped to an item in Spira
                int jamaReleaseId = (int)jamaRelease.Id;
                int jamaProjectId = this._JamaProject.ProjectNum;
                ReleaseMappingData.ReleaseMappingRow dataRow = this._releaseDataMapping.GetFromJamaId(jamaProjectId, jamaReleaseId);
                if (dataRow == null)
                {
                    //We need to insert a new release
                    RemoteRelease remoteRelease = new RemoteRelease();
                    if (String.IsNullOrEmpty(jamaRelease.Name))
                    {
                        remoteRelease.Name = "Unknown (Null)";
                    }
                    else
                    {
                        remoteRelease.Name = jamaRelease.Name;
                    }
                    if (!String.IsNullOrEmpty(jamaRelease.Name) && jamaRelease.Name.Length <= 10)
                    {
                        //If we have a release name that is short enough, use it for the version number
                        //otherwise let Spira auto-generate it
                        remoteRelease.VersionNumber = jamaRelease.Name;
                    }
                    else
                    {
                        //Currently "" denotes that we need to create a new version number
                        //in future versions of the API it should also support passing null
                        remoteRelease.VersionNumber = "";
                    }
                    remoteRelease.Description = jamaRelease.Description;
                    remoteRelease.ReleaseStatusId = (jamaRelease.Active) ? (int)SpiraProject.ReleaseStatusEnum.Planned : (int)SpiraProject.ReleaseStatusEnum.Completed;
                    remoteRelease.ReleaseTypeId = (int)SpiraProject.ReleaseTypeEnum.MajorRelease;

                    if (jamaRelease.ReleaseDate.HasValue)
                    {
                        //We assume that the release lasts 1-month by default
                        remoteRelease.StartDate = jamaRelease.ReleaseDate.Value.AddMonths(-1).Date;
                        remoteRelease.EndDate = jamaRelease.ReleaseDate.Value.Date;
                    }
                    else
                    {
                        //Just create a start/end date based on the current date
                        remoteRelease.StartDate = DateTime.UtcNow.Date;
                        remoteRelease.EndDate = DateTime.UtcNow.AddMonths(1).Date;
                    }

                    //Create the release
                    remoteRelease = this._spiraClient.Release_Create(remoteRelease, null);
                    this._releaseDataMapping.Add(
                        this._SpiraProject.ProjectNum,
                        remoteRelease.ReleaseId.Value,
                        jamaProjectId,
                        jamaReleaseId
                        );
                }
                else
                {
                    //We need to update an existing release (we leave the dates alone)
                    int spiraReleaseId = dataRow.SpiraReleaseId;
                    RemoteRelease remoteRelease = this._spiraClient.Release_RetrieveById(spiraReleaseId);
                    if (remoteRelease == null)
                    {
                        //Could not find release and it was mapped.
                        retStatus = FinalStatusEnum.Warning;
                    }
                    else
                    {
                        //Update the release in Spira
                        if (!String.IsNullOrEmpty(jamaRelease.Name))
                        {
                            remoteRelease.Name = jamaRelease.Name;
                            if (jamaRelease.Name.Length <= 10)
                            {
                                //If we have a release name that is short enough, use it for the version number
                                //otherwise let Spira auto-generate it
                                remoteRelease.VersionNumber = jamaRelease.Name;
                            }
                        }
                        remoteRelease.Description = jamaRelease.Description;
                        remoteRelease.Active = jamaRelease.Active;
                        this._spiraClient.Release_Update(remoteRelease);
                    }
                }

                if (ProcessThread.WantCancel)
                {
                    return FinalStatusEnum.Error;
                }
            }
            catch (Exception ex)
            {
                streamWriter.WriteLine("Unable to complete import: " + ex.Message);

                retStatus = FinalStatusEnum.Error;
            }
            return retStatus;
        }

		/// <summary>
        /// Processes a Jama item, taking care of the associated attachments, comments, etc.
        /// </summary>
		/// <param name="jamaItem">The item to process.</param>
        private FinalStatusEnum ProcessItem(StreamWriter streamWriter, JamaItem jamaItem)
        {
            FinalStatusEnum retStatus = FinalStatusEnum.OK;

            try
            {
                //We ignore the Defects, Issue, Task, Risk and Change Request items
                if (jamaItem.ItemTypeId.HasValue)
                {
                    if (jamaItem.ItemTypeId == 14 || (jamaItem.ItemTypeId >= 16 && jamaItem.ItemTypeId <= 19 || jamaItem.ItemTypeId == (int)JamaProjectInfo.Type.Bug || jamaItem.ItemTypeId == (int)JamaProjectInfo.Type.ChangeRequest))
                    {
                        return FinalStatusEnum.OK;
                    }
                }

                int? ExistingMapped = GetLinkedNumber(jamaItem);

                //See if we have a mapping entry
                if (ExistingMapped.HasValue)
                {
                    //Get the mapped requirement.
                    SpiraSoapService.RemoteRequirement existingReq = this._spiraClient.Requirement_RetrieveById(ExistingMapped.Value);

                    //If it no longer exists, just ignore (user needs to clear the mappings if they want to reimport)
                    if (existingReq != null)
                    {
                        //Update the requirement from the Jama item
                        SpiraSoapService.RemoteRequirement newReq = this.GenerateOrUpdateRequirement(jamaItem, existingReq);

                        //Update the requirement
                        this._spiraClient.Requirement_Update(newReq);
                    }
                    else
                    {
                        //Could not find requirement and it was mapped.
                        retStatus = FinalStatusEnum.Warning;
                    }
                }
                else
                {
                    //Create the requirement.
                    SpiraSoapService.RemoteRequirement newReq = this.GenerateOrUpdateRequirement(jamaItem, null);

                    //Get parent's ID. If it is null, ignore this requirement because the parent
                    //  folder was deleted. (We should always have a root, because of the RootReq
                    //  or the root folder.)
                    int? ParentID = GetParentLinkedNumber(jamaItem);
                    if (ParentID.HasValue)
                    {
                        //Insert new Requirement.
                        if (ParentID == -1)
                        {
                            //HACK: Force it to root.
                            newReq = this._spiraClient.Requirement_Create1(newReq, -100);
                        }
                        else
                        {
                            newReq = this._spiraClient.Requirement_Create2(newReq, ParentID);
                        }

                        this.SetLinkedNumber(jamaItem, this._SpiraProject.ProjectNum, newReq.RequirementId.Value);
                    }
                    else
                    {
                        //Could not find requirement's parent, and it was mapped.
                        retStatus = FinalStatusEnum.Warning;
                    }
                }

                if (ProcessThread.WantCancel)
                {
                    return FinalStatusEnum.Error;
                }
            }
            catch (Exception ex)
            {
                streamWriter.WriteLine("Unable to complete import: " + ex.Message);
                retStatus = FinalStatusEnum.Error;
            }
            return retStatus;
        }

        /// <summary>Searches the Tagged Values for the mapping ID.</summary>
        /// <param name="element">The element to search in.</param>
        /// <returns>Integer indicating the SpiraTeam Requirement ID, NULL if no mapping.</returns>
        private int? GetLinkedNumber(JamaItem jamaItem)
        {
            if (this._requirementDataMapping == null)
            {
                return null;
            }
            RequirementMappingData.RequirementMappingRow dataRow = this._requirementDataMapping.GetFromJamaId(jamaItem.ProjectId, jamaItem.Id);
            if (dataRow == null)
            {
                return null;
            }
            else
            {
                return dataRow.SpiraRequirementId;
            }
        }

        /// <summary>Gets the Spira Mapped ID for the parent item of the specified item.</summary>
        /// <param name="jamaItem">The Jama item</param>
        /// <returns>The Spira Mapping ID of the parent. -1 if the parent is a root item, NULL if mapping was deleted.</returns>
        private int? GetParentLinkedNumber(JamaItem jamaItem)
        {
            if (this._requirementDataMapping == null)
            {
                return null;
            }

            //See if we have a parent item or not
            if (jamaItem.ParentId.HasValue)
            {
                RequirementMappingData.RequirementMappingRow dataRow = this._requirementDataMapping.GetFromJamaId(jamaItem.ProjectId, jamaItem.ParentId.Value);
                if (dataRow == null)
                {
                    //If not, create a new requirement for this folder, if the parent exists.
                    JamaItem jamaParentItem = this._jamaClient.GetItem(jamaItem.ParentId.Value);
                    if (jamaParentItem == null)
                    {
                        //If the parent item no longer exists, just return -1
                        return null;
                    }
                    SpiraSoapService.RemoteRequirement parReq = this.GenerateOrUpdateRequirement(jamaParentItem, null);

                    //Get mapping for this item's parent.
                    int? itemParentParentMappedID = GetParentLinkedNumber(jamaParentItem);
                    //If the value is null, it was deleted, so do not create this and return NULL.
                    if (!itemParentParentMappedID.HasValue)
                    {
                        return null;
                    }
                    else
                    {
                        int? sentParentID = null;
                        if (itemParentParentMappedID > 0)
                            sentParentID = itemParentParentMappedID;

                        //Create requirement.
                        if (itemParentParentMappedID.Value < 0)
                        {
                            //HACK: Force it to root.
                            parReq = this._spiraClient.Requirement_Create1(parReq, -100);
                        }
                        else
                        {
                            parReq = this._spiraClient.Requirement_Create2(parReq, sentParentID);
                        }

                        //Create mapping.
                        this.SetLinkedNumber(jamaParentItem, this._SpiraProject.ProjectNum, parReq.RequirementId.Value);

                        //Return requirement ID. (It's the parent.)
                        return parReq.RequirementId;
                    }
                }
                else
                {
                    //Make sure the requirement still exists, otherwise return null
                    SpiraSoapService.RemoteRequirement checkReq = this._spiraClient.Requirement_RetrieveById(dataRow.SpiraRequirementId);
                    if (checkReq == null)
                    {
                        return null;
                    }
                    else
                    {
                        return dataRow.SpiraRequirementId;
                    }
                }
            }
            else
            {
                if (this._SpiraProject.RootReq > 0)
                {
                    return this._SpiraProject.RootReq;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>Generates a remote requirement from the specified item.</summary>
        /// <param name="jamaItem">The Jama item to create the requirement from.</param>
        /// <param name="existingReq">Pass an existing requirement to update, leave null to create new</param>
        /// <returns>RemoteRequirement</returns>
        private SpiraSoapService.RemoteRequirement GenerateOrUpdateRequirement(JamaItem jamaItem, SpiraSoapService.RemoteRequirement existingReq)
        {
            SpiraSoapService.RemoteRequirement newReq;
            if (existingReq == null)
            {
                newReq = new SpiraSoapService.RemoteRequirement();
                newReq.CustomProperties = new List<RemoteArtifactCustomProperty>();
            }
            else
            {
                newReq = existingReq;
            }
            if (String.IsNullOrEmpty(jamaItem.Name))
            {
                newReq.Name = "Unknown (Null)";
            }
            else
            {
                newReq.Name = jamaItem.Name;
            }
            newReq.Description = jamaItem.Description;
            newReq.RequirementTypeId = (int)SpiraProject.RequirementType.Feature;
            if (jamaItem.CreatedDate.HasValue)
            {
                newReq.CreationDate = jamaItem.CreatedDate.Value;
            }
            if (jamaItem.ModifiedDate.HasValue)
            {
                newReq.LastUpdateDate = jamaItem.ModifiedDate.Value;
            }
            if (existingReq == null)
            {
                //Set the concurrent Date for new items
                newReq.ConcurrencyDate = DateTime.UtcNow;
            }

            //Get the other fields
            bool documentKeyFound = false;
            foreach (KeyValuePair<string, object> field in jamaItem.Fields)
            {
                //Handle the known fields
                //Priority
                if (field.Key == "priority")
                {
                    if (field.Value != null && field.Value is Int32)
                    {
                        int jamaPriorityId = (int)field.Value;
                        switch ((JamaProjectInfo.Priority)jamaPriorityId)
                        {
                            case JamaProjectInfo.Priority.High:
                                newReq.ImportanceId = (int)SpiraProject.Importance.High;
                                break;

                            case JamaProjectInfo.Priority.Medium:
                                newReq.ImportanceId = (int)SpiraProject.Importance.Medium;
                                break;

                            case JamaProjectInfo.Priority.Low:
                                newReq.ImportanceId = (int)SpiraProject.Importance.Low;
                                break;
                        }
                    }
                }

                //Status
                if (field.Key == "status")
                {
                    if (field.Value != null && field.Value is Int32)
                    {
                        int jamaStatusId = (int)field.Value;
                        switch ((JamaProjectInfo.Status)jamaStatusId)
                        {
                            case JamaProjectInfo.Status.Draft:
                                newReq.StatusId = (int)SpiraProject.Status.Requested;
                                break;
                            case JamaProjectInfo.Status.Approved:
                                newReq.StatusId = (int)SpiraProject.Status.Accepted;
                                break;
                            case JamaProjectInfo.Status.Completed:
                                newReq.StatusId = (int)SpiraProject.Status.Completed;
                                break;
                            case JamaProjectInfo.Status.Rejected:
                                newReq.StatusId = (int)SpiraProject.Status.Rejected;
                                break;
                        }
                    }
                }
                
                //Document Key
                if (field.Key == "documentKey")
                {
                    if (field.Value != null && field.Value is String)
                    {
                        string documentKey = (string)field.Value;
                        if (!String.IsNullOrWhiteSpace(documentKey))
                        {
                            string jamaItemKey = jamaItem.Id.ToString();
                            RemoteArtifactCustomProperty customProp = new RemoteArtifactCustomProperty();
                            customProp.PropertyNumber = 2;
                            customProp.StringValue = documentKey;
                            newReq.CustomProperties.Add(customProp);
                            documentKeyFound = true;
                        }
                    }
                }

                //Release
                if (field.Key == "release")
                {
                    if (field.Value != null && field.Value is Int32)
                    {
                        int jamaReleaseId = (int)field.Value;
                        //See if we have a mapping for this release
                        ReleaseMappingData.ReleaseMappingRow dataRow = this._releaseDataMapping.GetFromJamaId(this._JamaProject.ProjectNum, jamaReleaseId);
                        if (dataRow == null)
                        {
                            newReq.ReleaseId = null;
                        }
                        else
                        {
                            newReq.ReleaseId = dataRow.SpiraReleaseId;
                        }
                    }
                }
            }

            //Now get the item document type and item document type category and store in text custom properties
            if (jamaItem.ItemTypeId.HasValue)
            {
                int itemTypeId = jamaItem.ItemTypeId.Value;
                JamaItemType itemType = this._jamaClient.GetItemType(itemTypeId);
                if (itemType != null)
                {
                    {
                        RemoteArtifactCustomProperty customProp = new RemoteArtifactCustomProperty();
                        customProp.PropertyNumber = 1;
                        customProp.StringValue = itemType.Display;
                        newReq.CustomProperties.Add(customProp);
                    }

                    //Also map to the nearest equivalent requirement type
                    switch ((JamaProjectInfo.Type)itemTypeId)
                    {
                        case JamaProjectInfo.Type.Feature:
                            newReq.RequirementTypeId = (int)SpiraProject.RequirementType.Feature;
                            break;
                        case JamaProjectInfo.Type.Requirement:
                        case JamaProjectInfo.Type.Epic:
                            newReq.RequirementTypeId = (int)SpiraProject.RequirementType.Need;
                            break;
                        case JamaProjectInfo.Type.UseCase:
                        case JamaProjectInfo.Type.UsageScenario:
                            newReq.RequirementTypeId = (int)SpiraProject.RequirementType.UseCase;
                            break;
                        case JamaProjectInfo.Type.UserStory:
                            newReq.RequirementTypeId = (int)SpiraProject.RequirementType.UserStory;
                            break;
                        default:
                            newReq.RequirementTypeId = (int)SpiraProject.RequirementType.Feature;
                            break;
                    }

                    //Also we need to add the jama ID as the Custom 02 custom property
                    //using format: <project-short-name>-<Jama item type key>-<number>
                    if (!documentKeyFound)
                    {
                        if (!String.IsNullOrWhiteSpace(this._projectShortName))
                        {
                            string jamaItemKey = jamaItem.Id.ToString();
                            RemoteArtifactCustomProperty customProp = new RemoteArtifactCustomProperty();
                            customProp.PropertyNumber = 2;
                            customProp.StringValue = this._projectShortName + "-" + itemType.TypeKey + "-" + jamaItemKey;
                            newReq.CustomProperties.Add(customProp);
                        }
                    }

                    if (!String.IsNullOrEmpty(itemType.Category))
                    {
                        RemoteArtifactCustomProperty customProp = new RemoteArtifactCustomProperty();
                        customProp.PropertyNumber = 3;
                        customProp.StringValue = itemType.Category;
                        newReq.CustomProperties.Add(customProp);
                    }
                }
            }

            return newReq;
        }

        /// <summary>Sets a mapping value on the specified item.</summary>
        /// <param name="jamaItem">The Jama item to set the mapping on.</param>
        /// <param name="spiraProjectid">The Spira project ID</param>
        /// <param name="spiraReqId">The Spira requirement ID to map to, NULL to delete the mapping.</param>
        private void SetLinkedNumber(JamaItem jamaItem, int spiraProjectid, int? spiraReqId)
        {
            //See if we have the add or remove case
            if (spiraReqId.HasValue)
            {
                //See if we have an existing item
                RequirementMappingData.RequirementMappingRow dataRow = this._requirementDataMapping.GetFromJamaId(jamaItem.ProjectId, jamaItem.Id);
                if (dataRow == null)
                {
                    this._requirementDataMapping.Add(spiraProjectid, spiraReqId.Value, jamaItem.ProjectId, jamaItem.Id);
                }
                else
                {
                    dataRow.SpiraProjectId = spiraProjectid;
                    dataRow.SpiraRequirementId = spiraReqId.Value;
                    dataRow.AcceptChanges();
                }
            }
            else
            {
                //Delete the item
                this._requirementDataMapping.Delete(jamaItem.ProjectId, jamaItem.Id);
            }
        }
	}
}
