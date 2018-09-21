using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.ServiceModel;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	internal partial class ProcessThread
	{

        private bool ConnectToSpira(StreamWriter streamWriter, out FinalStatusEnum Status)
		{
			Status = FinalStatusEnum.OK;
			this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = 1 });
            streamWriter.WriteLine("Connecting to Spira.");

			this._spiraClient = new SpiraSoapService.SoapServiceClient();
            this._spiraClient.Endpoint.Address = new EndpointAddress(this._SpiraProject.ServerURL + SpiraProject.URL_APIADD);
            BasicHttpBinding httpBinding = (BasicHttpBinding)this._spiraClient.Endpoint.Binding;
            WcfUtils.ConfigureBinding(httpBinding, this._spiraClient.Endpoint.Address.Uri);

			//Try connecting.
			try
			{
				bool CanConnect = this._spiraClient.Connection_Authenticate2(this._SpiraProject.UserName, this._SpiraProject.UserPass, Properties.Resources.Global_ApplicationName);
				if (CanConnect && !ProcessThread.WantCancel)
				{
					//Try connecting to the project, now.
					if (this._spiraClient.Connection_ConnectToProject(this._SpiraProject.ProjectNum) && !ProcessThread.WantCancel)
					{
						//Do they have a root requirement set? Verify it exists.
						if (this._SpiraProject.RootReq > 0 && !ProcessThread.WantCancel)
						{
							this._RootReq = this._spiraClient.Requirement_RetrieveById(this._SpiraProject.RootReq);
							if (this._RootReq == null)
							{
								string ErrorMsg = "Could not access root requirement RQ" + this._SpiraProject.RootReq.ToString();
								this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 1 });
								return false;
							}
						}
						else
						{
							if (ProcessThread.WantCancel)
							{
								this.ProgressUpdate(this, new ProgressArgs() { ErrorText = App.CANCELSTRING, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 1 });
								return false;
							}
						}

					}
					else
					{
						string ErrorMsg = "";
						if (ProcessThread.WantCancel)
							ErrorMsg = App.CANCELSTRING;

						else
							ErrorMsg = "Could not access specified SpiraTeam project.";
						this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 1 });
						return false;
					}
				}
				else
				{
					string ErrorMsg = "";
					if (ProcessThread.WantCancel)
						ErrorMsg = App.CANCELSTRING;
					else
						ErrorMsg = "Unable to log into the system.";
					this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 1 });
					return false;
				}

				this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = 1 });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
                streamWriter.WriteLine("Unable to log into SpiraTeam Server: " + ex.Message);

				string ErrorMsg = "Unable to log into the system:" + Environment.NewLine + ex.Message;
				this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 1 });
				return false;
			}
		}
	}
}
