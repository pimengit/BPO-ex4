using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_vS : SheetLogic
    {
        public ROUTE_vS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(600);
        }

        public override bool Compute()
        {
            return (!OR(1) && V(2) && !V(3) && V(4)) || (!OR(1) && V(5) && V(6) && V(7)) || (!OR(1) && V(2) && !V(4));
        }
    }
}
