using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SPEED_FH : SheetLogic
    {
        public SPEED_FH()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1)) || (!V(2) && OR(3) && V(7)) || (!V(4) && OR(5) && V(7)) || (V(6));
        }
    }
}
