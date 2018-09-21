using System.Windows.Controls;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    /// <summary>
    /// Interaction logic for DefaultConfiguration1.xaml
    /// </summary>
    public partial class DefaultConfiguration1 : UserControl, IProcedureComponent
    {
        public DefaultConfiguration1()
        {
            InitializeComponent();
        }

        #region IProcedureComponent Members

        public string KeyText
        {
            get { return "Configuration 1"; }
        }

        #endregion
    }
}
