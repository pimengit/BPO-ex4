using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_ROG : SheetLogic
    {
        public GEN_ROG()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2) && !OR(3) && !OR(4) && V(5) && !V(6) && !V(8)) || (!V(2) && !V(7) && V(8));
        }
    }
}
