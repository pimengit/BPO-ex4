using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KN_DKN : SheetLogic
    {
        public KN_DKN()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return AND(1) && !V(2) && V(3) && !V(4) && V(5) && V(6) && !V(7) && AND(8) && V(12) ||
                   V(4) && V(5) && V(6) && !V(7) && AND(8) ||
                   V(5) && V(6) && !V(7) && AND(8) && V(11) ||
                   V(5) && V(6) && !AND(8) && !AND(9) && !V(10) && V(11) ||
                   V(1) && !V(4) && V(6) && !V(7) && AND(8);
        }
    }
}
