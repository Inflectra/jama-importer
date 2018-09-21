using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.ServiceModel;

using Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.JamaClient;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for cntrlOptions_Jama.xaml
	/// </summary>
	public partial class cntrlOptions_Jama : UserControl, IProcedureComponent
	{
        public cntrlOptions_Jama()
		{
			InitializeComponent();

			ToolTip serverNotification = new ToolTip();
            serverNotification.Content = "Base URL of the Jama instance. Examples:" + Environment.NewLine + "https://vdpoo.jamacloud.com" + Environment.NewLine;
			this.txbServer.ToolTip = serverNotification;

			this.btnConnect.Tag = false;
            this.cmbProjectList.ItemsSource = new List<JamaProjectInfo>();

            //Populate from settings
            this.txbServer.Text = Properties.Settings.Default.JamaUrl;
            this.txbUserID.Text = Properties.Settings.Default.JamaLogin;
            if (String.IsNullOrEmpty(Properties.Settings.Default.JamaPassword))
            {
                this.chkRememberPassword.IsChecked = false;
                this.txbUserPass.Password = "";
            }
            else
            {
                this.chkRememberPassword.IsChecked = true;
                this.txbUserPass.Password = Properties.Settings.Default.JamaPassword;
            }
		}

		#region IProcedureComponent Members

		string IProcedureComponent.KeyText
		{
			get { return "Jama Contour Configuration"; }
		}

		#endregion

		#region txbRootReq Numeric Handlers
		/// <summary>Hit when the user hits a key in the numeric control.</summary>
		/// <param name="sender">txbRootReq</param>
		/// <param name="e">EventArgs</param>
		private void spira_ReqID_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if ((e.Key < Key.D0 || e.Key > Key.D9) &&
				(e.Key != Key.Back && e.Key != Key.Delete && e.Key != Key.System) &&
				(e.Key != Key.Left && e.Key != Key.Right) &&
				(e.Key != Key.OemMinus || e.Key != Key.Subtract))
			{
				if (e.Key >= Key.NumPad0 || e.Key <= Key.NumPad9)
				{ }
				else
					e.Handled = true;
			}
		}

		#endregion

		/// <summary>Hit when the user changes the selected Project.</summary>
		/// <param name="sender">cmbProjectList</param>
		/// <param name="e">EventArgs</param>
		private void cmbProjectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.cmbProjectList.SelectedItem != null)
			{
				this.msgStatus.Text = "Click 'Next' to continue...";
				this.msgStatus.ToolTip = null;
			}
			else
			{
				this.msgStatus.Text = "You must select a valid Jama Contour project.";
				this.msgStatus.ToolTip = null;
			}
		}

		/// <summary>Hit when the users wants to get projects from the server or cancel an existing connection.</summary>
		/// <param name="sender">btnConnect</param>
		/// <param name="e">EventArgs</param>
		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
            try
            {
                e.Handled = true;
                //They asked for it!
                if ((bool)btnConnect.Tag)
                {
                    //Ignore the operation
                }
                else
                {
                    //Error check, first.
                    if (this.CheckEntryFields())
                    {
                        //Set display.
                        this.setDisplayforProgress(true);

                        //Save in settings
                        Properties.Settings.Default.JamaUrl = this.txbServer.Text.Trim();
                        Properties.Settings.Default.JamaLogin = this.txbUserID.Text.Trim();
                        Properties.Settings.Default.JamaPassword = (chkRememberPassword.IsChecked.Value) ? this.txbUserPass.Password : "";

                        //Fire off a new client.
                        this.msgStatus.Text = "Connecting to server...";
                        JamaManager jamaManager = new JamaManager(Properties.Settings.Default.JamaUrl, Properties.Settings.Default.JamaLogin, Properties.Settings.Default.JamaPassword);
                        List<JamaProject> projects = jamaManager.GetProjects();

                        //Update status.
                        this.msgStatus.Text = "Projects retrieved from server.";
                        this.msgStatus.ToolTip = null;
                        this.setDisplayforProgress(false);

                        //Load combobox.
                        List<JamaProjectInfo> list = new List<JamaProjectInfo>();
                        list.Clear();
                        if (projects != null && projects.Count > 0)
                        {
                            foreach (JamaProject project in projects)
                            {
                                JamaProjectInfo newProj = new JamaProjectInfo();
                                if (project.Fields != null)
                                {
                                    newProj.ProjectName = project.ProjectKey + " - " + project.Fields.Name;
                                }
                                else
                                {
                                    newProj.ProjectName = project.ProjectKey;
                                }
                                newProj.ProjectNum = project.Id;
                                newProj.UserName = this.txbUserID.Text.Trim();
                                newProj.UserPass = this.txbUserPass.Password;
                                newProj.ServerURL = this.txbServer.Text.Trim();

                                list.Add(newProj);
                            }
                            this.grdAvailProjs.IsEnabled = true;
                            this.cmbProjectList.ItemsSource = list;
                        }
                        else
                        {
                            this.msgStatus.Text = "No projects available for that user.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.msgStatus.Text = "Error connecting to server.";
                if (ex.InnerException == null)
                {
                    this.msgStatus.ToolTip = ex.Message;
                }
                else
                {
                    this.msgStatus.ToolTip = ex.Message + "\n (" + ex.InnerException.Message + ")";
                }
                this.setDisplayforProgress(false);
            }
		}

		/// <summary>Checks the three server entry fields for proper entry.</summary>
		/// <returns>True if they pass validation, false otherwise.</returns>
		private bool CheckEntryFields()
		{
			bool retOK = true;
			string erMsg = "";

			if (string.IsNullOrEmpty(this.txbServer.Text.Trim()) ||
				this.txbServer.Text.Trim().ToLowerInvariant().EndsWith(".asmx") ||
				this.txbServer.Text.Trim().ToLowerInvariant().EndsWith(".aspx"))
			{
				this.txbServer.Tag = "1";
				retOK = false;
				erMsg += "Server URL is required, and should not end in .asmx or .aspx, and should be the root URL of your installation." + Environment.NewLine;
			}
			if (string.IsNullOrEmpty(this.txbUserID.Text.Trim()))
			{
				this.txbUserID.Tag = "1";
				retOK = false;
				erMsg += "User ID is required." + Environment.NewLine;
			}
			if (string.IsNullOrEmpty(this.txbUserPass.Password.Trim()))
			{
				this.txbUserPass.Tag = "1";
				retOK = false;
				erMsg += "Password is required.";
			}

			if (!retOK)
			{
				this.msgStatus.Text = "Please correct highlighted fields.";
				this.msgStatus.ToolTip = erMsg.Trim();
			}

			return retOK;
		}

		/// <summary>Hit when text is changed in one of the server boxes. Clears message, error highlight, and disabled Selected Project.</summary>
		/// <param name="sender">TextBox</param>
		/// <param name="e">EventArgs</param>
		private void txbEntry_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			//Clear tag & message.
			((Control)sender).Tag = null;
			this.msgStatus.Text = "";
			this.msgStatus.ToolTip = null;
			//Null out available projects.
			this.cmbProjectList.SelectedIndex = -1;
			this.grdAvailProjs.IsEnabled = false;
		}

		/// <summary>Called to put the form into/out of a progress state.</summary>
		/// <param name="inProgress">True if changing to an 'In Progress' state. False to return to default.</param>
		private void setDisplayforProgress(bool inProgress)
		{
			if (inProgress)
			{
				this.btnConnect.Content = "_Cancel";
				this.btnConnect.Tag = true;
				this.barProg.Visibility = System.Windows.Visibility.Visible;
				this.txbServer.IsEnabled = false;
				this.txbUserID.IsEnabled = false;
				this.txbUserPass.IsEnabled = false;
			}
			else
			{
				this.btnConnect.Content = "_Get Projects";
				this.btnConnect.Tag = false;
				this.barProg.Visibility = System.Windows.Visibility.Hidden;
				this.txbServer.IsEnabled = true;
				this.txbUserID.IsEnabled = true;
				this.txbUserPass.IsEnabled = true;
			}
		}

		/// <summary>Hit when a textblock's IsEnabled changes to show as 'disabled'.</summary>
		/// <param name="sender">TextBlock</param>
		/// <param name="e">EventArgs</param>
		private void TextBlock_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			((TextBlock)sender).Opacity = ((((TextBlock)sender).IsEnabled) ? 1 : .5);
		}

		/// <summary>Checks that information is filled out and good.</summary>
		public bool AreSettingsCorrect
		{
			get
			{
				bool iniCheck = CheckEntryFields();
				if (this.cmbProjectList.SelectedItem == null)
					iniCheck = false;

				return iniCheck;
			}
		}

		/// <summary>Returns the JamaProject that is selected, or null if error or to set the controls blank.</summary>
		internal JamaProjectInfo SelectedProject
		{
			get
			{
                if (this.cmbProjectList.SelectedItem != null && this.cmbProjectList.SelectedItem.GetType() == typeof(JamaProjectInfo))
				{
                    JamaProjectInfo retProj = (JamaProjectInfo)this.cmbProjectList.SelectedItem;
					return retProj;
				}
				else
					return null;
			}
			set
			{
                if (value != null && value.GetType() == typeof(JamaProjectInfo))
				{
					this.txbServer.Text = value.ServerURL;
					this.txbUserID.Text = value.UserName;
					this.txbUserPass.Password = value.UserPass;
                    ((List<JamaProjectInfo>)this.cmbProjectList.ItemsSource).Clear();
                    ((List<JamaProjectInfo>)this.cmbProjectList.ItemsSource).Add(value);
					this.cmbProjectList.SelectedIndex = 0;
					this.grdAvailProjs.IsEnabled = true;
				}
				else
				{
					//Null passed, clear the fields.
					this.cmbProjectList.Items.Clear();
					this.txbServer.Text = "";
					this.txbUserID.Text = "";
					this.txbUserPass.Password = "";
					this.grdAvailProjs.IsEnabled = false;
				}
			}
		}
	}
}
