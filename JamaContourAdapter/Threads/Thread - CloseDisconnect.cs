using System;
using System.Collections.Generic;
using System.Text;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI;
using System.Windows.Threading;
using System.Threading;
using System.IO;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter
{
	internal partial class ProcessThread
	{
        private void CloseDisconnect(StreamWriter streamWriter, out FinalStatusEnum Status)
		{
			this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = 0, Status = ItemProgress.ProcessStatusEnum.Processing, TaskNum = 3 });
            streamWriter.WriteLine("Closing and Disconnecting.");

			try
			{
                //Disconnect from Jama
                this._jamaClient = null;

				//Disconnect from Spira.
                if (this._spiraClient != null)
                {
                    this._spiraClient.Connection_Disconnect();
                    this._spiraClient.Close();
                }
			}
			catch (Exception ex)
			{
                streamWriter.WriteLine(ex.Message);
			}

			this.ProgressUpdate(this, new ProgressArgs() { ErrorText = "", Progress = 0, Status = ItemProgress.ProcessStatusEnum.Success, TaskNum = 3 });
			Status = FinalStatusEnum.OK;
		}
	}
}
