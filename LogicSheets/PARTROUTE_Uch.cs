using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class PARTROUTE_Uch : SheetLogic
    {
        public PARTROUTE_Uch()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(300);
        }

        public override bool Compute()
        {
            return (V(1) && V(4) && !OR(5)) 
                || (!V(1) && AND(2) && !AND(3) && V(4) && !OR(5) && !V(6) && V(7)) 
                || (V(1) && !AND(3) && V(4) && !OR(5) && !V(6) && V(7));
        }
    }
}
