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

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for wpfMasterForm.xaml
	/// </summary>
	public partial class wpfMasterForm : Window
	{
		#region Constructors
		public wpfMasterForm()
		{
			InitializeComponent();
			this.Visibility = System.Windows.Visibility.Collapsed;
			this.Closed += new EventHandler(Window_Closed);
			this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(Window_IsVisibleChanged);

            //Create the notify icon.
            this.SetNotifyIconProperties();

            //Create the master form.
            this.SetWizardWindowProperties();

            //Show the wizard.
            this.wizard.Show();
		}

		#endregion

		#region Master Window Events
		/// <summary>His when the window is finally unloaded to properly remove the NotifyIcon.</summary>
		/// <param name="sender">wpfMasterForm</param>
		/// <param name="e">EventArgs</param>
		private void Window_Closed(object sender, EventArgs e)
		{
			//If this is closing, close the notification icon.
			this.notifyIcon1.Container.Dispose();
		}

		/// <summary>Hit whenever the master form becomes visible again, for some reason.</summary>
		/// <param name="sender">wpfMasterForm</param>
		/// <param name="e">EventArgs</param>
		private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.IsVisible)
				this.Visibility = Visibility.Collapsed;
		}

		#endregion
	}
}
