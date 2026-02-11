using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_KNO : SheetLogic
    {
        public ROUTE_KNO()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(600);
        }

        public override bool Compute()
        {
            return (!V(1) && V(3) && !V(4) && V(5)) || (V(6)) || (V(7)) || (OR(2));
        }
    }
}
