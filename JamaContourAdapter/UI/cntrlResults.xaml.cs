using System.Windows.Controls;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class cntrlResults : UserControl, IProcedureComponent
	{
		public cntrlResults()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members

		string IProcedureComponent.KeyText
		{
			get { return "Welcome!"; }
		}

		#endregion

		public string SetFinishStatus
		{
			set
			{
				this.txbFinishStatus.Text = value;
			}
		}

		public string SetFinishMessage
		{
			set
			{
				this.txbFinishMessage.Text = value;
			}
		}

		public bool IsMinimizeSelected
		{
			get
			{
				return this.chkMinTray.IsChecked.Value;
			}
			set
			{
				this.chkMinTray.IsChecked = value;
			}
		}
	}
}
