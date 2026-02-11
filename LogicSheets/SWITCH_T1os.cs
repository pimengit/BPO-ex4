using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SWITCH_T1os : SheetLogic
    {
        public SWITCH_T1os()
        {
            OnDelay  = TimeSpan.FromMilliseconds(3000);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && !V(6) && V(7)) || (!V(2) && !V(3) && !V(4) && !V(5));
        }
    }
}
