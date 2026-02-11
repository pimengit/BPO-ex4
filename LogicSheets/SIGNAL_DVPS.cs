using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_DVPS : SheetLogic
    {
        public SIGNAL_DVPS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(2) && !AND(3)) || (!AND(3) && V(4)) || (V(1) && !AND(3));
        }
    }
}
