using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_PL2 : SheetLogic
    {
        public ROUTESECT_PL2()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(4) && V(5)) || (OR(6) && V(7)) || (V(3)) || (V(2)) || (V(1));
        }
    }
}
