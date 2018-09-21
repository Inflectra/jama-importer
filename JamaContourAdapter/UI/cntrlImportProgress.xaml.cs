using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm;
using System.Threading;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for cntrlImportProgress.xaml
	/// </summary>
	public partial class cntrlImportProgress : UserControl, IProcedureProcessComponent
	{
		//Private storage.
		SpiraProject _spiraProject;
        JamaProjectInfo _jamaProject;
		Thread _Thread;
		ProcessThread _ThreadClass;

		private List<ProcedureCompleteDelegate> procedureCompleteDelegates = new List<ProcedureCompleteDelegate>();

		internal cntrlImportProgress()
		{
			InitializeComponent();
		}

		#region IProcedureProcessComponent Members

		List<ProcedureCompleteDelegate> IProcedureProcessComponent.ProcedureCompleteDelegates
		{
			get
			{
				return procedureCompleteDelegates;
			}
		}

		void IProcedureProcessComponent.startProcedure()
		{
			// Using thread.start, this is where the call to the main importing function is done.
			ProcessThread.WantCancel = false;
			this._ThreadClass = new ProcessThread(this._spiraProject, this._jamaProject);
			this._ThreadClass.ProgressUpdate += new EventHandler<ProcessThread.ProgressArgs>(_ThreadClass_ProgressUpdate);
			this._ThreadClass.ProgressFinished += new EventHandler<ProcessThread.ProgressArgs>(_ThreadClass_ProgressFinished);
			this._Thread = new Thread(this._ThreadClass.StartProcess);
			this._Thread.Name = "SpiraTeam Jama Contour Adapter: Worker Thread";
			this._Thread.Start();
		}

		private delegate void _ThreadClass_ProgressFinishedCallback(object sender, ProcessThread.ProgressArgs e);
		void _ThreadClass_ProgressFinished(object sender, ProcessThread.ProgressArgs e)
		{
			if (this.Dispatcher.CheckAccess())
			{
				//Set progress bar color.
				switch (e.Status)
				{
					case ItemProgress.ProcessStatusEnum.Error:
						this.barProgress.Foreground = new SolidColorBrush(Colors.Red);
						break;
					case ItemProgress.ProcessStatusEnum.Processing:
						this.barProgress.Foreground = new SolidColorBrush(Colors.Yellow);
						break;
				}
				if (this.barProgress.IsIndeterminate) this.barProgress.IsIndeterminate = false;

				for (int i = 0; i < procedureCompleteDelegates.Count; i++)
				{
					procedureCompleteDelegates[i].Invoke();
				}
			}
			else
			{
				_ThreadClass_ProgressFinishedCallback callB = new _ThreadClass_ProgressFinishedCallback(this._ThreadClass_ProgressFinished);
				this.Dispatcher.Invoke(callB, new object[] { sender, e });
			}
		}

		/// <summary>Delegate to handle the intra-thread call to _ThreadClass_ProgressUpdate</summary>
		/// <param name="sender">ThreadClass</param>
		/// <param name="e">ProcessThread.ProgressArgs</param>
		private delegate void _ThreadClass_ProgressUpdate_Callback(object sender, ProcessThread.ProgressArgs e);
		/// <summary>Hit in the other thread when an update is requested.</summary>
		/// <param name="sender">ThreadClass</param>
		/// <param name="e">ProcessThread.ProgressArgs</param>
		private void _ThreadClass_ProgressUpdate(object sender, ProcessThread.ProgressArgs e)
		{
			if (this.Dispatcher.CheckAccess())
			{
				//Update form!
				// - Progress bar.
				if (e.Progress > -2)
				{
					this.barProgress.IsIndeterminate = (e.Progress == -1);
					this.barProgress.Value = ((e.Progress == -1) ? 0 : e.Progress);
				}

				// - Task item.
				ItemProgress selectItem = (ItemProgress)this.itemsToDo.Children[e.TaskNum];
				selectItem.SetActionStatus = e.Status;
				selectItem.SetErrorString = e.ErrorText;
			}
			else
			{
				_ThreadClass_ProgressUpdate_Callback callB = new _ThreadClass_ProgressUpdate_Callback(this._ThreadClass_ProgressUpdate);
				this.Dispatcher.Invoke(callB, new object[] { sender, e });
			}
		}

		bool IProcedureProcessComponent.cancelProcedure()
		{
			try
			{
				ProcessThread.WantCancel = true;
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		#endregion

		#region IProcedureComponent Members

		string IProcedureComponent.KeyText
		{
			get { return "Importing Requirements"; }
		}

		#endregion

		/// <summary>Hit when the progress bar changes value, to update the label.</summary>
		/// <param name="sender">ProgressBar</param>
		/// <param name="e">EventArgs</param>
		private void barProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			this.txbProgress.Visibility = ((this.barProgress.IsIndeterminate) ? Visibility.Collapsed : Visibility.Visible);
			this.txbProgress.Text = ((double)barProgress.Value * (double)100).ToString("##0") + "%";
		}

		/// <summary>Used for setting the SpiraTeam project after it's been selected.</summary>
		internal SpiraProject SpiraProject
		{
			set
			{
				this._spiraProject = value;
			}
		}

		/// <summary>Used for setting the Jama Contour project after it's been selected.</summary>
		internal JamaProjectInfo JamaProject
		{
			set
			{
				this._jamaProject = value;
			}
		}

		/// <summary>Reports the finished status.</summary>
		public string GetFinishStatus
		{
			get
			{
				return this._ThreadClass.FinishStatus;
			}
		}

		/// <summary>Gets the finished message.</summary>
		public string GetFinishMessage
		{
			get
			{
				return this._ThreadClass.FinishedMessage;
			}
		}
	}
}
