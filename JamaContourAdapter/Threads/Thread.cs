using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI;
using System.Windows.Threading;
using System.Threading;

using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.SpiraSoapService;
using System.IO;


namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	internal partial class ProcessThread
	{
		private SpiraProject _SpiraProject;
		private JamaProjectInfo _JamaProject;

        private SpiraSoapService.SoapServiceClient _spiraClient;
        private JamaManager _jamaClient;

		private SpiraSoapService.RemoteRequirement _RootReq;

		//Keeps track of statuses for when we're asked for the summary page.
        private FinalStatusEnum CanConnectToJama = FinalStatusEnum.OK;
		private FinalStatusEnum CanConnectToSpira = FinalStatusEnum.OK;
		private FinalStatusEnum CanTransfer = FinalStatusEnum.OK;
		private FinalStatusEnum CanDisconnect = FinalStatusEnum.OK;

		public event EventHandler<ProgressArgs> ProgressUpdate;
		public event EventHandler<ProgressArgs> ProgressFinished;

		//Trying to cancel?
		public static bool WantCancel;

		/// <summary>Creates a new instance of the class to perform the work.</summary>
        /// <param name="spiraProject">The SpiraTeam project to use for importing.</param>
        /// <param name="jamaProject">The Jama Contour project used for importing.</param>
		/// <param name="ProgressForm">The ProgressForm. Should probably be a delegate, but the form will work for now.</param>
		/// <param name="ExistingMappings">Mappings already defined for this combination.</param>
		public ProcessThread(SpiraProject spiraProject, JamaProjectInfo jamaProject)
		{
            this._JamaProject = jamaProject;
            this._SpiraProject = spiraProject;
		}

		/// <summary>Called to start processing.</summary>
		public void StartProcess()
		{
            //First open up the textfile that we will log information to (used for debugging purposes)
            string debugFile = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\Spira_JamaImport.log";
            StreamWriter streamWriter = File.CreateText(debugFile);

            try
            {
                // Stage 1 - Connect to Jama
                if (this.ConnectToJama(streamWriter, out this.CanConnectToJama) && !WantCancel)
                {
                    // Stage 2 - Connect to Spira
                    if (this.ConnectToSpira(streamWriter, out this.CanConnectToSpira) && !WantCancel)
                    {
                        // Stage 3 - Import Data
                        this.ProcessImport(streamWriter, out this.CanTransfer);
                        // Stage 4 - Disconnect & Close
                        this.CloseDisconnect(streamWriter, out this.CanDisconnect);
                    }
                    else
                    {
                        //Error connecting.
                        // Stage 4 - Disconnect & Close
                        this.CloseDisconnect(streamWriter, out this.CanDisconnect);
                    }
                }
                else
                {
                    //Error opening.
                    // Stage 4 - Disconnect & Close
                    this.CloseDisconnect(streamWriter, out this.CanDisconnect);
                }

                //Call the finish.
                ProgressArgs finishArgs = new ProgressArgs();
                if (this.CanConnectToSpira == FinalStatusEnum.Error || this.CanConnectToJama == FinalStatusEnum.Error || this.CanTransfer == FinalStatusEnum.Error || this.CanDisconnect == FinalStatusEnum.Warning)
                    finishArgs.Status = ItemProgress.ProcessStatusEnum.Error;
                else
                    if (this.CanConnectToSpira == FinalStatusEnum.Warning || this.CanConnectToJama == FinalStatusEnum.Warning || this.CanTransfer == FinalStatusEnum.Warning || this.CanDisconnect == FinalStatusEnum.Warning)
                        finishArgs.Status = ItemProgress.ProcessStatusEnum.Processing;

                finishArgs.Progress = -2;
                this.ProgressFinished(this, finishArgs);
            }
            finally
            {
                streamWriter.Close();
            }
		}
      
		public string FinishStatus
		{
			get
			{
				if (this.CanTransfer == FinalStatusEnum.Error)
					return "Error importing requirements.";
				else if (this.CanTransfer == FinalStatusEnum.Warning)
					return "Importing requirements successful.";
                else if (this.CanConnectToSpira == FinalStatusEnum.Error || this.CanConnectToSpira == FinalStatusEnum.Warning)
					return "Error connecting to SpiraTeam.";
                else if (this.CanConnectToJama == FinalStatusEnum.Error)
					return "Error connecting to Jama Contour.";
				else
					return "Importing Requirements successful.";
			}
		}
		public string FinishedMessage
		{
			get
			{
				if (ProcessThread.WantCancel)
					return "The import process was canceled. Some items may have been imported, but not all.";
				if (this._SpiraProject.RootReq == -1)
					return "Mappings were erased from the specified Jama Contour Project. However, if the process was canceled, not all mappings were removed.";
				if (this.CanTransfer == FinalStatusEnum.Error)
					return "Error during import. One or more requirements from the file could not be imported. Check the Application Event log for any errors and contact support if necessary.";
				if (this.CanTransfer == FinalStatusEnum.Warning)
					return "Some requirements were deleted off of the SpiraTeam server and were not re-imported. All other requirements were imported successfully.";
				if (this.CanConnectToSpira == FinalStatusEnum.Error)
					return "Could not connect to the SpiraTeam server. Check that the login/username is correct, that the account has proper access to create and modify requirements, and that the Root Requirement is correct, if it is set.";
                if (this.CanConnectToSpira == FinalStatusEnum.Warning)
					return "Could not locate the root requirement for importing. Verify that the requested requirement exists in the selected project.";
                if (this.CanConnectToJama == FinalStatusEnum.Error)
                    return "Could not connect to the Jama Contour server. Check that the login/username is correct, that the account has proper access to view requirements in the project.";
				else
					return "All requirements from the Jama Contour project were imported successfully.";
			}
		}


		/// <summary>Class to hold progress information.</summary>
		internal class ProgressArgs : EventArgs
		{
			public int TaskNum;
			public ItemProgress.ProcessStatusEnum Status;
			public double Progress;
			public string ErrorText;
		}

		private enum FinalStatusEnum
		{
			OK = 0,
			Error = 1,
			Warning = 2
		}
	}
}
