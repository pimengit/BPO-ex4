using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SWITCH_MSD : SheetLogic
    {
        public SWITCH_MSD()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) && V(2) && V(3) && V(4) && V(5) && V(6) && V(7) && V(8) && V(9) && V(10) && V(11) && V(12) && V(13) && V(14) && V(15) && V(16) && V(17) && V(18) && V(19) && V(20) && V(21) && V(22) && V(23) && V(24) && V(25) && V(26) && V(27);
        }
    }
}
