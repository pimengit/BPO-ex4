using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_40CUM4 : SheetLogic
    {
        public ROUTESECT_40CUM4()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) && V(2) && V(3) && V(4) && V(5) && V(6) && V(7) && V(8) && V(9) && V(10) && V(11);
        }
    }
}
