using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KURBEL_KKUK : SheetLogic
    {
        public KURBEL_KKUK()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(5) && V(6) && V(7) && !V(9) && !V(10)) || (!V(2) && V(6) && !V(8) && V(9) && V(10)) || (!V(2) && !V(4) && V(6) && !V(9) && V(10)) || (V(2) && V(3) && V(6) && !V(9));
        }
    }
}
