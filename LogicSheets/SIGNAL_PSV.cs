using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_PSV : SheetLogic
    {
        public SIGNAL_PSV()
        {
            OnDelay  = TimeSpan.FromMilliseconds(1500);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && !V(2) && !V(4) && !OR(5) && !V(6) && !V(7) && !OR(8)) || (!V(1) && V(3) && !V(4) && !OR(5) && !V(6) && !V(7) && !OR(8));
        }
    }
}
