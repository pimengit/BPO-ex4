using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_S : SheetLogic
    {
        public ROUTE_S()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (AND(1) && !OR(2) && AND(3) && V(4) && V(5) && V(6) && V(7) && V(11)) 
                || (AND(1) && !OR(2) && AND(3) && V(4) && V(5) && V(6) && OR(8) && V(11)) 
                || (AND(1) && !OR(2) && AND(3) && V(4) && V(5) && V(6) && V(9)) 
                || (AND(1) && !OR(2) && AND(3) && V(4) && V(5) && V(6) && OR(10) && V(11));
        }
    }
}
