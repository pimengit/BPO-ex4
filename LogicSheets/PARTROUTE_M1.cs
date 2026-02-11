using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class PARTROUTE_M1 : SheetLogic
    {
        public PARTROUTE_M1()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(4) && V(9)) || (!V(1) && V(4) && V(8)) || (!V(1) && !V(3) && V(4) && V(5) && !V(6) && V(7)) || (!V(1) && !V(2) && V(3) && V(4) && V(5) && V(6) && V(7));
        }
    }
}
