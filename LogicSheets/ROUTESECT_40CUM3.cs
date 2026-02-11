using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_40CUM3 : SheetLogic
    {
        public ROUTESECT_40CUM3()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!OR(2) && AND(3) && V(4) && V(5) && V(6) && V(7) && V(8) && V(9)) || (V(1) && !OR(2) && AND(3) && V(5) && V(6) && V(7) && V(8) && V(9));
        }
    }
}
