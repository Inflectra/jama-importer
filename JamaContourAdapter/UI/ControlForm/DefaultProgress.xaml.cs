using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    /// <summary>
    /// Interaction logic for DefaultProgress.xaml
    /// </summary>
    public partial class DefaultProgress : UserControl, IProcedureProcessComponent
    {
        private List<ProcedureCompleteDelegate> procedureCompleteDelegates = new List<ProcedureCompleteDelegate>();
        private int counter = 0;
        private delegate void RefreshDelegate();
        private Thread t = null;

        public DefaultProgress()
        {
            InitializeComponent();
        }

        private void updateProgressLoop()
        {
            try
            {
                while (counter < 100)
                {
                    counter++;

                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(updateProgress));

                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch (Exception)
                    {
                    }
                }

                counter = 100;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(updateProgress));

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(updateFinished));
            }
            catch (Exception)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new RefreshDelegate(updateFinished));
            }
        }

        private void updateProgress()
        {
            progressBar.Value = counter;
        }

        private void updateFinished()
        {
            for (int i = 0; i < procedureCompleteDelegates.Count; i++)
            {
                procedureCompleteDelegates[i].Invoke();
            }
        }

        #region IProcedureProcessComponent Members

        public List<ProcedureCompleteDelegate> ProcedureCompleteDelegates
        {
            get { return procedureCompleteDelegates; }
        }

        public void startProcedure()
        {
            t = new Thread(new ThreadStart(updateProgressLoop));
            t.Name = "Default Master Progress Thread";
            t.Start();
        }

        public bool cancelProcedure()
        {
            try
            {
                Thread.Sleep(2000);
            }
            catch (Exception)
            {
            }

            try
            {
                t.Abort();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region IProcedureComponent Members

        public string KeyText
        {
            get { return "Progress"; }
        }

        #endregion
	}
}
