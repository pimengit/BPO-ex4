using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_KN2 : SheetLogic
    {
        public ROUTE_KN2()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1)) 
                || (!AND(2) && !OR(3) && !V(5) && !V(6) && !OR(7) && V(8))
                || (!AND(2) && !OR(3) && !V(4) && !V(6) && V(8))
                || (!AND(2) && !OR(3) && !V(4) && V(5) && V(8));
        }
    }
}
