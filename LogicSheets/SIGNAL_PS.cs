using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_PS : SheetLogic
    {
        public SIGNAL_PS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(12)) 
                || (!V(1) && !V(4) && !V(5) && !V(7) && V(8) && OR(9) && !OR(10) && V(13) && V(14) && !V(15)) 
                || (V(8) && OR(9) && V(11) && !V(15)) 
                || (!V(1) && V(5) && V(7) && V(8) && OR(9) && !OR(10) && !V(15)) 
                || (!V(1) && V(2) && V(5) && V(8) && OR(9) && !OR(10) && V(13) && !V(15)) 
                || (V(3) && !V(4) && !V(5) && !V(6) && !V(7) && V(8) && OR(9) && !OR(10) && V(13) && !V(14) && !V(15));
        }
    }
}
