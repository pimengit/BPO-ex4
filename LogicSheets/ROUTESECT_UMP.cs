using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_UMP : SheetLogic
    {
        public ROUTESECT_UMP()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && V(5)) || (OR(2) && V(5)) || (V(4) && V(5) && V(7)) || (OR(3) && !V(4) && V(5) && V(6)) || (OR(2) && !V(4) && V(5) && V(6));
        }
    }
}
