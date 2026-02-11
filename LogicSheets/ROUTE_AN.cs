using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_AN : SheetLogic
    {
        public ROUTE_AN()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!OR(1) && V(2) && !OR(4) && V(11)) 
                || (!OR(1) && V(3) && !OR(4) && !V(5) && V(11)) 
                || (!OR(1) && !V(3) && !OR(4) && V(5) && V(11)) 
                || (!OR(1) && !OR(4) && V(6) && V(7)) 
                || (!OR(1) && !OR(4) && V(6) && !V(8) && V(9) && V(11)) 
                || (!OR(1) && !OR(4) && OR(10));
        }
    }
}
