using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_UPS : SheetLogic
    {
        public SIGNAL_UPS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && !V(2) && OR(3) && !OR(4) && !V(5) && V(6));
        }
    }
}
