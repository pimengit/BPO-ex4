using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KGU_NKGU : SheetLogic
    {
        public KGU_NKGU()
        {
            OnDelay  = TimeSpan.FromSeconds(0.6);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2)) || (!V(1) && V(2));
        }
    }
}
