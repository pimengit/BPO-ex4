using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_ROG : SheetLogic
    {
        public GEN_ROG()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !OR(2) && !OR(3) && V(4) && !V(5) && !V(7)) ||
                   (!V(1) && !V(6) && V(7));
        }
    }
}
