using System.Windows.Controls;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    /// <summary>
    /// Interaction logic for DefaultConfirmation.xaml
    /// </summary>
    public partial class DefaultConfirmation : UserControl, IProcedureComponent
    {
        public DefaultConfirmation()
        {
            InitializeComponent();
        }

        #region IProcedureComponent Members

        public string KeyText
        {
            get { return "Confirmation"; }
        }

        #endregion
    }
}
