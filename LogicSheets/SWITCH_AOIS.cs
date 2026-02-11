using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SWITCH_AOIS : SheetLogic
    {
        public SWITCH_AOIS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(2) && V(3)) || (OR(1) && !V(3));
        }
    }
}
