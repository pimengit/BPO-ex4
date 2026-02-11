using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class PARTROUTE_UVzVch : SheetLogic
    {
        public PARTROUTE_UVzVch()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2) && !OR(3) && !OR(4) && !OR(5) && !AND(6) && !V(7) && V(8)) 
                || (V(1) && OR(3) && OR(4) && !AND(6) && !V(7) && V(8));
        }
    }
}
