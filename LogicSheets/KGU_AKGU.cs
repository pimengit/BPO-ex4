using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KGU_AKGU : SheetLogic
    {
        public KGU_AKGU()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2) && V(3)) 
                || (V(1) && !V(2) && V(4)) 
                || (V(5));
        }
    }
}
