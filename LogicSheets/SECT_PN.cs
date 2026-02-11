using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_PN : SheetLogic
    {
        public SECT_PN()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(5) && V(6) && !V(8) && V(9) && !V(10) && !V(11) && AND(12) && AND(13) && V(14) && !V(15)) 
                || (!V(4) && V(5) && V(6) && !V(7) && !V(8) && !V(10) && !V(11) && AND(12) && AND(13) && V(15)) 
                || (!V(4) && V(5) && V(6) && !V(8) && V(10) && !V(11) && AND(12) && AND(13) && V(15)) 
                || (V(3) && V(4) && V(6) && !V(8) && !V(10) && !V(11) && AND(12) && AND(13)) 
                || (V(2) && V(4) && V(6) && !V(8) && !V(10) && !V(11) && AND(12) && AND(13) && V(15)) 
                || (V(1) && V(5) && V(6) && !V(8) && !V(11) && AND(12) && AND(13));
        }
    }
}
