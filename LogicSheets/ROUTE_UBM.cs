using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_UBM : SheetLogic
    {
        public ROUTE_UBM()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (AND(1) && !V(2) && V(3) && AND(4) && V(5) && AND(6) && V(7) && V(8) && V(10) && AND(11) && V(12)) 
                || (AND(1) && !V(2) && V(3) && AND(4) && V(5) && AND(6) && V(9) && V(10) && AND(11) && V(12));
    }
}
