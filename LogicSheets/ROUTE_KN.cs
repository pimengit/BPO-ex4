using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_KN : SheetLogic
    {
        public ROUTE_KN()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (AND(1) && V(3) && !V(4) && !V(5) && !V(6) && !OR(7) && V(8) && !OR(9) && !OR(10) && !V(11) && V(12) && !V(15)) 
                || (V(4) && V(5) && !OR(7) && V(8) && !OR(9) && !OR(10) && !V(11) && V(12)) 
                || (!OR(10) && !V(12) && !V(13) && !OR(14) && V(15)) 
                || (!OR(10) && !V(11) && !V(13) && V(15)) 
                || (!OR(10) && !V(11) && V(12) && V(15)) 
                || (AND(2) && !V(4) && !V(5) && !V(6) && !OR(7) && V(8) && !OR(9) && !OR(10) && !V(11) && V(12));
        }
    }
}
