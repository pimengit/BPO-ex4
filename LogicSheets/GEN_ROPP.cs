using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_ROPP : SheetLogic
    {
        public GEN_ROPP()
        {
            OnDelay  = TimeSpan.FromSeconds(0.45);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2)) || (!V(1) && V(2));
        }
    }
}
