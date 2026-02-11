using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_RMSV : SheetLogic
    {
        public SIGNAL_RMSV()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (AND(1) && V(2));
        }
    }
}
