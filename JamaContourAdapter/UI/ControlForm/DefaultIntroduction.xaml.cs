using System.Windows.Controls;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    /// <summary>
    /// Interaction logic for DefaultIntroduction.xaml
    /// </summary>
    public partial class DefaultIntroduction : UserControl, IProcedureComponent
    {
        public DefaultIntroduction()
        {
            InitializeComponent();
        }

        #region IProcedureComponent Members

        public string KeyText
        {
            get { return "Introduction"; }
        }

        #endregion
    }
}
