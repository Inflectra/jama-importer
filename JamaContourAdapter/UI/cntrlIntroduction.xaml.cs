using System.Windows.Controls;
using Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.UI
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class cntrlIntroduction : UserControl, IProcedureComponent
	{
		public cntrlIntroduction()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members

		string IProcedureComponent.KeyText
		{
			get { return "Welcome!"; }
		}

		#endregion
	}
}
