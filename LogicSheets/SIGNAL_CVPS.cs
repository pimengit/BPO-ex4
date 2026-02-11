using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_CVPS : SheetLogic
    {
        public SIGNAL_CVPS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(1200);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(2) && !V(3) && !OR(4) && !OR(5) && !OR(6) && V(7)) || (V(1) && V(3) && OR(4) && !OR(5) && !OR(6));
        }
    }
}
