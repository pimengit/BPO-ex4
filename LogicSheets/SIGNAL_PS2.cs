using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_PS2 : SheetLogic
    {
        public SIGNAL_PS2()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2) && !V(5) && V(9) && OR(10) && !OR(11) && V(12) && !V(13) && !V(14) && !V(15)) 
                || (!V(2) && !V(5) && !V(6) && !V(8) && V(9) && OR(10) && !OR(11) && V(13) && V(14) && !V(15)) 
                || (!V(2) && V(6) && V(8) && V(9) && OR(10) && !OR(11) && !V(15)) 
                || (!V(2) && V(3) && V(6) && V(9) && OR(10) && !OR(11) && V(13) && !V(15)) 
                || (V(4) && !V(5) && !V(6) && !V(7) && !V(8) && V(9) && OR(10) && !OR(11) && V(13) && !V(14) && !V(15));
        }
    }
}
