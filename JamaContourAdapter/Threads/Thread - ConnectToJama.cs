using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI;
using System.Windows.Threading;
using System.Threading;
using System.IO;
using System.ServiceModel;

using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	internal partial class ProcessThread
	{
		private bool ConnectToJama(StreamWriter streamWriter, out FinalStatusEnum Status)
		{
			Status = FinalStatusEnum.OK;
			this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = 0 });
            streamWriter.WriteLine("Connecting to Jama.");

            this._jamaClient = new JamaManager(this._JamaProject.ServerURL, this._JamaProject.UserName, this._JamaProject.UserPass);

			//Try connecting.
			try
			{
                bool CanConnect = this._jamaClient.TestConnection();
				if (CanConnect && !ProcessThread.WantCancel)
				{
					//Try connecting to the project, now.
                    if (this._jamaClient.GetProject(this._JamaProject.ProjectNum) != null && !ProcessThread.WantCancel)
					{
						if (ProcessThread.WantCancel)
						{
							this.ProgressUpdate(this, new ProgressArgs() { ErrorText = App.CANCELSTRING, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 0 });
							return false;
						}
					}
					else
					{
						string ErrorMsg = "";
						if (ProcessThread.WantCancel)
							ErrorMsg = App.CANCELSTRING;

						else
							ErrorMsg = "Could not access specified Jama Contour project.";
						this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 0 });
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
					this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 0 });
					return false;
				}

				this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = -1, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = 0 });
				return true;
			}
			catch (Exception ex)
			{
				//Log error.
                streamWriter.WriteLine("Unable to log into Jama Contour Server: " + ex.Message);
				string ErrorMsg = "Unable to log into the system:" + Environment.NewLine + ex.Message;
				this.ProgressUpdate(this, new ProgressArgs() { ErrorText = ErrorMsg, Progress = -1, Status = ItemProgress.ProcessStatusEnum.Error, TaskNum = 0 });
				return false;
			}
		}
	}
}
