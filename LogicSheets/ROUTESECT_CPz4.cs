using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_CPz4 : SheetLogic
    {
        public ROUTESECT_CPz4()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && V(2) && V(3) && AND(4)) || (V(1) && AND(4) && V(5) && V(6));
        }
    }
}
