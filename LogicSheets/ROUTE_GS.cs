using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_GS : SheetLogic
    {
        public ROUTE_GS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(450);
        }

        public override bool Compute()
        {
            return (V(1) && !OR(5) && V(6)) || (V(2) && !OR(3) && !OR(4) && !OR(5) && V(6)) || (!OR(3) && !OR(4) && !OR(5) && V(6) && V(7));
        }
    }
}
