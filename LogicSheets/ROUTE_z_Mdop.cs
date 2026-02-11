using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_z_Mdop : SheetLogic
    {
        public ROUTE_z_Mdop()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(8) && V(9) && !AND(10)) 
                || (V(7) && V(9) && !AND(10)) 
                || (!V(1) && !V(2) && !V(3) && !OR(5) && V(6) && V(8)) 
                || (!V(1) && !V(4) && !OR(5) && V(6) && V(8));
        }
    }
}
