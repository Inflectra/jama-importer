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
using System.Drawing;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for wpfMasterForm.xaml
	/// </summary>
	public partial class wpfMasterForm : Window
	{
		//Notify Icon Menus.
		private System.Windows.Forms.ContextMenu menu_Hidden;
		private System.Windows.Forms.ContextMenu menu_Shown;
		private System.Windows.Forms.NotifyIcon notifyIcon1;

		private bool isResetting = false;

		private void SetNotifyIconProperties()
		{
			//Create our Form-is-Hidden menu
			this.menu_Hidden = new System.Windows.Forms.ContextMenu();
			this.menu_Hidden.Popup += new EventHandler(notifyMenu_Popup);
			this.menu_Hidden.MenuItems.Add(new System.Windows.Forms.MenuItem("&Show Window") { Name = "mnuShow", DefaultItem = true });
			this.menu_Hidden.MenuItems.Add(new System.Windows.Forms.MenuItem("Rerun &Import") { Name = "mnuRunAgain" });
			this.menu_Hidden.MenuItems.Add(new System.Windows.Forms.MenuItem("-"));
			this.menu_Hidden.MenuItems.Add(new System.Windows.Forms.MenuItem("E&xit") { Name = "mnuExit" });
			for (int i = 0; i < this.menu_Hidden.MenuItems.Count; i++)
				this.menu_Hidden.MenuItems[i].Click += new EventHandler(notifyMenu_Click);
			//Create our Form-is-Shown menu
			this.menu_Shown = new System.Windows.Forms.ContextMenu();
			this.menu_Shown.Popup += new EventHandler(notifyMenu_Popup);
			this.menu_Shown.MenuItems.Add(new System.Windows.Forms.MenuItem("Main Window Open...") { DefaultItem = true, Enabled = false });

			// Create the NotifyIcon.
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(new System.ComponentModel.Container());
			notifyIcon1.ContextMenu = this.menu_Hidden;

			// The Text property sets the text that will be displayed, in a tooltip, when the mouse hovers over the systray icon.
			notifyIcon1.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Inflectra.SpiraTest.AddOns.JamaContourAdapter._Resources.Spira.ico"));
			notifyIcon1.Text = Properties.Resources.Global_ApplicationName;
			notifyIcon1.Visible = true;
			notifyIcon1.DoubleClick += new EventHandler(notifyMenu_DoubleClick);
		}

		#region NotifyIcon Events
		/// <summary>Hit when the menu is displayed. Will enable/disable commands depending on form status.</summary>
		/// <param name="sender">ContextMenu</param>
		/// <param name="e">EventArgs</param>
		private void notifyMenu_Popup(object sender, EventArgs e)
		{
			if (this.wizard.IsVisible)
			{
				this.notifyIcon1.ContextMenu = this.menu_Shown;
			}
			else
			{
				this.notifyIcon1.ContextMenu = this.menu_Hidden;
			}
		}

		/// <summary>Hit when they want to re-open the wizard.</summary>
		/// <param name="sender">notifyMenu</param>
		/// <param name="e">EventArgs</param>
		private void notifyMenu_DoubleClick(object sender, EventArgs e)
		{
			if (!this.wizard.IsVisible)
			{
				this.SetWizardWindowProperties();
				this.wizard.Show();
			}
		}

		/// <summary>Hit when an menu item is selected.</summary>
		/// <param name="sender">menuItem</param>
		/// <param name="e">EventArgs</param>
		private void notifyMenu_Click(object sender, EventArgs e)
		{
			System.Windows.Forms.MenuItem clickedMenu = (System.Windows.Forms.MenuItem)sender;

			switch (clickedMenu.Name)
			{
				case "mnuExit":
					{
						this.wizard.ignoreMin = true;
						this.wizard.Close();
						this.Close();
					}
					break;
				case "mnuShow":
					{
						//Reset the wizard and show.
						this.wizard.ignoreMin = true;
						this.isResetting = true;
						this.wizard.Close();
						this.SetWizardWindowProperties();
						this.wizard.currentScreen = this.wiz_optJama; //Advance past introduction.
						this.wizard.Show();
						this.isResetting = false;
					}
					break;
				case "mnuRunAgain":
					{
						this.wizard.currentScreen = this.wiz_Confirm;
						this.wiz_Progress.taskConnectSpira.SetActionStatus = ItemProgress.ProcessStatusEnum.None;
						this.wiz_Progress.taskDisconnect.SetActionStatus = ItemProgress.ProcessStatusEnum.None;
						this.wiz_Progress.taskImport.SetActionStatus = ItemProgress.ProcessStatusEnum.None;
						this.wiz_Progress.taskConnectJama.SetActionStatus = ItemProgress.ProcessStatusEnum.None;
						this.wiz_Progress.barProgress.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Green);
						this.wiz_Progress.barProgress.Value = 0;
						this.wiz_Progress.barProgress.IsIndeterminate = true;
						this.wizard.Show();
					}
					break;
			}
		}
		#endregion
	}
}
