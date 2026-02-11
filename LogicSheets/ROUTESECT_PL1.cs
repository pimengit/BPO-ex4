using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_PL1 : SheetLogic
    {
        public ROUTESECT_PL1()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(5) && V(6)) || (OR(7) && V(8)) || (OR(9) && V(10)) || (OR(11) && V(12)) || (V(4)) || (V(3)) || (V(2)) || (V(1));
        }
    }
}
