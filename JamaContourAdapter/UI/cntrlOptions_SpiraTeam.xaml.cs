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
using System.ServiceModel;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.HelperClasses;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for cntrlOptions.xaml
	/// </summary>
	public partial class cntrlOptions_SpiraTeam : UserControl, IProcedureComponent
	{
		private const string URI_APPEND = "/Services/v5_0/SoapService.svc";
		private SpiraSoapService.SoapServiceClient _Client = null;
		private int _numRunning;
		//private int _userNum;

		public cntrlOptions_SpiraTeam()
		{
			InitializeComponent();

			ToolTip serverNotification = new ToolTip();
			serverNotification.Content = "URL of the server, not including 'Login.aspx' or 'ImportExport.asmx'. Examples:" + Environment.NewLine + "http://server/SpiraTeam" + Environment.NewLine + "https://localhost:10569/SpiraTeam";
			this.txbServer.ToolTip = serverNotification;

			//Make the client.
			this._Client = new SpiraSoapService.SoapServiceClient();
			this.btnConnect.Tag = false;
			this.cmbProjectList.ItemsSource = new List<SpiraProject>();

            //Populate from settings
            this.txbServer.Text = Properties.Settings.Default.SpiraUrl;
            this.txbUserID.Text = Properties.Settings.Default.SpiraLogin;
            if (String.IsNullOrEmpty(Properties.Settings.Default.SpiraPassword))
            {
                this.chkRememberPassword.IsChecked = false;
                this.txbUserPass.Password = "";
            }
            else
            {
                this.chkRememberPassword.IsChecked = true;
                this.txbUserPass.Password = Properties.Settings.Default.SpiraPassword;
            }
        }

		#region IProcedureComponent Members

		string IProcedureComponent.KeyText
		{
			get { return "SpiraTeam Configuration"; }
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

		/// <summary>Hit when the user cuts and pastes text into the control.</summary>
		/// <param name="sender">txbRootReq</param>
		/// <param name="e">EventArgs</param>
		private void spira_ReqID_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			int tryParse = 0;
			if (e.Text != "-")
			{
				if (!int.TryParse(e.Text, out tryParse))
				{
					e.Handled = true;
				}
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
				this.msgStatus.Text = "You must select a valid SpiraTeam project.";
				this.msgStatus.ToolTip = null;
			}
		}

		/// <summary>Hit when the users wants to get projects from the server or cancel an existing connection.</summary>
		/// <param name="sender">btnConnect</param>
		/// <param name="e">EventArgs</param>
		private void btnConnect_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			//They asked for it!
			if ((bool)btnConnect.Tag)
			{
                //Cancel the operation
                this._Client.Abort();
			}
			else
			{
				//Error check, first.
				if (this.CheckEntryFields())
				{
					//Set display.
					this.setDisplayforProgress(true);

                    //Save in settings
                    Properties.Settings.Default.SpiraUrl = this.txbServer.Text.Trim();
                    Properties.Settings.Default.SpiraLogin = this.txbUserID.Text.Trim();
                    Properties.Settings.Default.SpiraPassword = (chkRememberPassword.IsChecked.Value) ? this.txbUserPass.Password : "";

					//Fire off a new client.
					this._numRunning = 0;
					this.msgStatus.Text = "Connecting to server...";
                    this._Client = new SpiraSoapService.SoapServiceClient();
                    this._Client.Endpoint.Address = new EndpointAddress(this.txbServer.Text + URI_APPEND);
                    BasicHttpBinding httpBinding = (BasicHttpBinding)this._Client.Endpoint.Binding;
                    WcfUtils.ConfigureBinding(httpBinding, this._Client.Endpoint.Address.Uri);
                    this._Client.Connection_Authenticate2Completed +=new EventHandler<SpiraSoapService.Connection_Authenticate2CompletedEventArgs>(Client_Connection_Authenticate2Completed);
                    this._Client.Connection_Authenticate2Async(this.txbUserID.Text, this.txbUserPass.Password, Properties.Resources.Global_ApplicationName, this._numRunning++);
				}
			}
		}

		/// <summary>Hit when the client finishes logging in.</summary>
		/// <param name="sender">Client</param>
		/// <param name="e">EventArgs</param>
		private void Client_Connection_Authenticate2Completed(object sender, SpiraSoapService.Connection_Authenticate2CompletedEventArgs e)
		{
			if (e.Error == null)
			{
				if (!e.Cancelled)
				{
                    //Not canceled, no error.
                    this.msgStatus.Text = "Downloading project list...";
                    this.msgStatus.ToolTip = null;
                    SpiraSoapService.SoapServiceClient client = (SpiraSoapService.SoapServiceClient)sender;
                    client.Project_RetrieveCompleted += new EventHandler<SpiraSoapService.Project_RetrieveCompletedEventArgs>(client_Project_RetrieveCompleted);
                    client.Project_RetrieveAsync(this._numRunning++);
                }
				else
				{
					this.msgStatus.Text = "Connection canceled.";
					this.msgStatus.ToolTip = null;
					this.setDisplayforProgress(false);
				}
			}
			else
			{
				this.msgStatus.Text = "Error connecting to server.";
				this.msgStatus.ToolTip = e.Error.Message;
				this.setDisplayforProgress(false);
			}
		}

		/// <summary>Hit when the client is finished getting projects from the server.</summary>
		/// <param name="sender">Client</param>
		/// <param name="e">EventArgs</param>
		private void client_Project_RetrieveCompleted(object sender, SpiraSoapService.Project_RetrieveCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				if (!e.Cancelled)
				{
					//Update status.
					this.msgStatus.Text = "Projects retrieved from server.";
					this.msgStatus.ToolTip = null;
					//Load combobox.
					List<SpiraProject> list = new List<SpiraProject>();
					list.Clear();
					if (e.Result.Count > 0)
					{
						foreach (SpiraSoapService.RemoteProject project in e.Result)
						{
							SpiraProject newProj = new SpiraProject();
							newProj.ProjectName = project.Name;
							newProj.ProjectNum = project.ProjectId.Value;
							newProj.UserName = this.txbUserID.Text.Trim();
							newProj.UserPass = this.txbUserPass.Password;
							newProj.ServerURL = new Uri(this.txbServer.Text.Trim());

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
				else
				{
					this.msgStatus.Text = "Connection canceled.";
					this.msgStatus.ToolTip = null;
				}
			}
			else
			{
				this.msgStatus.Text = "Error getting projects from server.";
				this.msgStatus.ToolTip = e.Error.Message;
			}
			this.setDisplayforProgress(false);
		}

		/// <summary>Checks the three server entry fields for proper entry.</summary>
		/// <returns>True if they pass validation, false otherwise.</returns>
		private bool CheckEntryFields()
		{
			bool retOK = true;
			string erMsg = "";

			if (string.IsNullOrEmpty(this.txbServer.Text.Trim()) ||
				this.txbServer.Text.Trim().ToLowerInvariant().EndsWith(".asmx") ||
                this.txbServer.Text.Trim().ToLowerInvariant().EndsWith(".svc") ||
                this.txbServer.Text.Trim().ToLowerInvariant().EndsWith(".aspx"))
			{
				this.txbServer.Tag = "1";
				retOK = false;
				erMsg += "Server URL is required, and should not end in .asmx, .svc or .aspx, and should be the root URL of your installation." + Environment.NewLine;
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

		/// <summary>Returns the SpiraProject that is selected, or null if error or to set the controls blank.</summary>
		internal SpiraProject SelectedProject
		{
			get
			{
				if (this.cmbProjectList.SelectedItem != null && this.cmbProjectList.SelectedItem.GetType() == typeof(SpiraProject))
				{
					SpiraProject retProj = (SpiraProject)this.cmbProjectList.SelectedItem;
					if (!string.IsNullOrEmpty(this.txbRootReq.Text))
						retProj.RootReq = int.Parse(this.txbRootReq.Text);

					return retProj;
				}
				else
					return null;
			}
			set
			{
				if (value != null && value.GetType() == typeof(SpiraProject))
				{
					this.txbServer.Text = value.ServerURL.AbsoluteUri;
					this.txbUserID.Text = value.UserName;
					this.txbUserPass.Password = value.UserPass;
					((List<SpiraProject>)this.cmbProjectList.ItemsSource).Clear();
					((List<SpiraProject>)this.cmbProjectList.ItemsSource).Add(value);
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
					this.txbRootReq.Text = "";
					this.grdAvailProjs.IsEnabled = false;
				}
			}
		}

		/// <summary>Gets or sets the root Requirement ID #.</summary>
		internal int? RootRequirement
		{
			get
			{
				int tryInt = 0;
				int? retInt = null;
				if (!int.TryParse(this.txbRootReq.Text, out tryInt))
					return retInt;
				else
				{
					retInt = tryInt;
					return retInt;
				}
			}
			set
			{
				if (value.HasValue)
					this.txbRootReq.Text = value.Value.ToString();
				else
					this.txbRootReq.Text = "";
			}
		}
	}
}
