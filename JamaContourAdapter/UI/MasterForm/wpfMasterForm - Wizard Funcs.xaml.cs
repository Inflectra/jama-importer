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
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for wpfMasterForm.xaml
	/// </summary>
	public partial class wpfMasterForm : Window
	{
		// Main Wizard form.
		private ControlForm.ProcedureDialog wizard;

		// Settings pages.
		private UI.cntrlIntroduction wiz_Intro;
		private UI.cntrlOptions_SpiraTeam wiz_optSpira;
		private UI.cntrlOptions_Jama wiz_optJama;
		private UI.cntrlConfirmation wiz_Confirm;
		private UI.cntrlImportProgress wiz_Progress;
		private UI.cntrlResults wiz_Results;

		/// <summary>Creates the wizard window.</summary>
		private void SetWizardWindowProperties()
		{
			//Add config forms..
			this.wiz_Intro = new UI.cntrlIntroduction();
			this.wiz_optSpira = new UI.cntrlOptions_SpiraTeam();
            this.wiz_optJama = new UI.cntrlOptions_Jama();
			this.wiz_Confirm = new UI.cntrlConfirmation();
			this.wiz_Progress = new UI.cntrlImportProgress();
			this.wiz_Results = new UI.cntrlResults();
			List<ControlForm.IProcedureComponent> wizard_Steps = new List<ControlForm.IProcedureComponent>();
            wizard_Steps.Add(this.wiz_optJama);
			wizard_Steps.Add(this.wiz_optSpira);

			//Individual page events.
			this.wiz_Progress.IsVisibleChanged += new DependencyPropertyChangedEventHandler(wiz_Progress_IsVisibleChanged);
			this.wiz_Results.IsVisibleChanged += new DependencyPropertyChangedEventHandler(wiz_Results_IsVisibleChanged);

			this.wizard = new ControlForm.ProcedureDialog("SpiraTeam Jama Contour Adapter",
				this.wiz_Intro,
				wizard_Steps,
				this.wiz_Confirm,
				this.wiz_Progress,
				this.wiz_Results);
			this.wizard.NavigationChange += new RoutedEventHandler(wizard_NavigationChange);
			//this.wizard.Deactivated += new EventHandler(wizard_Deactivated);
			this.wizard.Closed += new EventHandler(wizard_Closed);
			this.wizard.Closing += new System.ComponentModel.CancelEventHandler(wizard_Closing);
			this.wizard.ShowProcessWarning = false;
			this.wizard.HeaderTitle = Properties.Resources.Global_ApplicationName;
			this.wizard.HeaderDescription = "Imports Requirements from Jama Contour into Inflectra SpiraTeam";
		}

		#region Wizard Events
		/// <summary>Hit when the Results dialog is displayed. Used to load progress info into it.</summary>
		/// <param name="sender">cntrlResults</param>
		/// <param name="e">eventArgs</param>
		void wiz_Results_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (wiz_Results.IsVisible)
			{
				//Requested to view Results. Grab from Progress.
				this.wiz_Results.SetFinishStatus = this.wiz_Progress.GetFinishStatus;
				this.wiz_Results.SetFinishMessage = this.wiz_Progress.GetFinishMessage;
			}
		}

        /* Not needed for Jama Contour Integration */
        //#region Prevent hidden EA.EXE to take focus.
        ///* To stop the Wizard from losing focus to the hidden copy of EA. */
        //[DllImport("user32.dll")]
        //static extern IntPtr GetForegroundWindow();
        //[DllImport("user32.dll")]
        //static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        //[DllImport("kernel32.dll")]
        //static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);
        //[DllImport("psapi.dll")]
        //static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
        //void wizard_Deactivated(object sender, EventArgs e)
        //{
        //    const int nChars = 1024;
        //    IntPtr handle = IntPtr.Zero;
        //    UInt32 access = 1040;
        //    //Get the window handle.
        //    handle = GetForegroundWindow();

        //    //Get the process.
        //    uint processId = 0;
        //    StringBuilder filename = new StringBuilder(nChars);
        //    GetWindowThreadProcessId(handle, out processId);
        //    IntPtr hProcess = OpenProcess(access, 0, processId);
        //    GetModuleFileNameEx(hProcess, IntPtr.Zero, filename, nChars);

        //    if (filename.ToString().ToLowerInvariant().EndsWith("ea.exe"))
        //    {
        //        wizard.Activate();
        //        wizard.Focus();
        //    }
        //}
        //#endregion

		/// <summary>Hit when the Progress first becomes visible.</summary>
		/// <param name="sender">wiz_Progress</param>
		/// <param name="e">EventArgs</param>
		void wiz_Progress_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.wiz_Progress.IsVisible)
			{
				//Set the action, in case they wanted to use the egg.
				if (this.wiz_optSpira.SelectedProject.RootReq == -1)
					this.wiz_Progress.taskImport.SetActionName = "Removing Existing Mappings";
				this.wiz_Progress.SpiraProject = this.wiz_optSpira.SelectedProject;
				this.wiz_Progress.JamaProject = this.wiz_optJama.SelectedProject;
			}
		}

		/// <summary>Hit when the wizard is navigated to another screen.</summary>
		/// <param name="sender">ProgressForm</param>
		/// <param name="e">EventArgs</param>
		private void wizard_NavigationChange(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			if ((int)sender > -1)
			{
				if (this.wiz_optSpira.IsVisible)
				{
					if (!this.wiz_optSpira.AreSettingsCorrect)
					{
						System.Windows.MessageBox.Show("Errors on the this step need to be corrected before you can continue.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
						e.Handled = false;
					}
					else
					{
						//Displaying the summary dialog. Input summary statements.
						// - Spira Server info.
						string txtSumm = "SpiraTeam Server:" + Environment.NewLine;
						txtSumm += "---------------- " + Environment.NewLine;
						txtSumm += "Server:       " + this.wiz_optSpira.SelectedProject.ServerURL.AbsoluteUri + Environment.NewLine;
						txtSumm += "Project Name: " + this.wiz_optSpira.SelectedProject.ProjectName + Environment.NewLine;
						txtSumm += "Login ID:     " + this.wiz_optSpira.SelectedProject.UserName + Environment.NewLine;
						txtSumm += "Root Folder:  " + this.wiz_optSpira.RootRequirement.ToString() + Environment.NewLine;
						txtSumm += Environment.NewLine;
                        txtSumm += "Jama Contour Server:" + Environment.NewLine;
                        txtSumm += "---------------- " + Environment.NewLine;
                        txtSumm += "Server:       " + this.wiz_optJama.SelectedProject.ServerURL + Environment.NewLine;
                        txtSumm += "Project Name: " + this.wiz_optJama.SelectedProject.ProjectName + Environment.NewLine;
                        txtSumm += "Login ID:     " + this.wiz_optJama.SelectedProject.UserName + Environment.NewLine;

						this.wiz_Confirm.SummaryText = txtSumm;
					}
				}
				else if (this.wiz_optJama.IsVisible)
				{
                    if (!this.wiz_optJama.AreSettingsCorrect)
                    {
                        System.Windows.MessageBox.Show("Errors on the this step need to be corrected before you can continue.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        e.Handled = false;
                    }
				}
			}
			else
			{ }
		}

		/// <summary>Hit when the wizard form is closing.</summary>
		/// <param name="sender">ProgressForm</param>
		/// <param name="e">EventArgs</param>
		private void wizard_Closed(object sender, EventArgs e)
		{
			if (!this.isResetting)
				this.Close();
		}

		/// <summary>Hit when the wizard is about to close. Checks the status of the 'Minimize' checkbox.</summary>
		/// <param name="sender">wizard</param>
		/// <param name="e">EventArgs</param>
		private void wizard_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.wizard.ignoreMin && this.wiz_Results.IsMinimizeSelected)
			{
				//Otherwise, override closed and hide it.
				e.Cancel = true;
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object o)
				{
					this.wizard.Visibility = Visibility.Collapsed;
					return null;
				}, null);
			}
		}
		#endregion
	}
}
