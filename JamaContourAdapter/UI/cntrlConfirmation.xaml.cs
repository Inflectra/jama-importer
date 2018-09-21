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
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for cntrlConfirmation.xaml
	/// </summary>
	public partial class cntrlConfirmation : UserControl, IProcedureComponent
	{
		public cntrlConfirmation()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members

		string IProcedureComponent.KeyText
		{
			get { return "Confirm Import"; }
		}

		#endregion

		public string SummaryText
		{
			get
			{
				return this.txbSummary.Text;
			}
			set
			{
				this.txbSummary.Text = value;
			}
		}
	}
}
