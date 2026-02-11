using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_SU : SheetLogic
    {
        public ROUTE_SU()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return !V(1) && !OR(2) && AND(3) && V(4) && V(5);
        }
    }
}
