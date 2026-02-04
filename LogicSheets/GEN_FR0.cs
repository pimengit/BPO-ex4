using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_FR0 : SheetLogic
    {
        public GEN_FR0()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(2) && V(3) && V(4) && V(5) && V(6) && !V(7) && !V(8) && V(9)) ||
                   (!V(2) && !V(3) && !V(4) && V(5) && V(6) && !V(7) && !V(8) && V(9)) ||
                   (!V(2) && V(7) && !V(8) && V(9)) ||
                   (!V(2) && V(8) && V(9)) ||
                   (V(1) && V(2) && V(9));
        }
    }
}
