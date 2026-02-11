using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_2ZHLv : SheetLogic
    {
        public ROUTE_2ZHLv()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && V(2)) || (!OR(3) && AND(4) && V(5) && AND(6) && V(7));
        }
    }
}
