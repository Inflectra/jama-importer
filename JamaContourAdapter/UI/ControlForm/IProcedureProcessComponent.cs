using System.Collections.Generic;

namespace Inflectra.SpiraTest.AddOns.JamaContourAdapter.ControlForm
{
    public delegate void ProcedureCompleteDelegate();

    public interface IProcedureProcessComponent : IProcedureComponent
    {
        List<ProcedureCompleteDelegate> ProcedureCompleteDelegates
        {
            get;
        }

        void startProcedure();
        bool cancelProcedure();
    }
}
