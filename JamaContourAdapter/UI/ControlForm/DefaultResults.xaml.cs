using System.Windows.Controls;


namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    /// <summary>
    /// Interaction logic for DefaultResults.xaml
    /// </summary>
    public partial class DefaultResults : UserControl, IProcedureComponent
    {
        public DefaultResults()
        {
            InitializeComponent();
        }

        #region IProcedureComponent Members

        public string KeyText
        {
            get { return "Results"; }
        }

        #endregion
    }
}
