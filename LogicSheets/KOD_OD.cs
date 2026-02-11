using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KOD_OD : SheetLogic
    {
        public KOD_OD()
        {
            OnDelay = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(6) && V(9) && !V(10) && !V(11) && !V(12)) 
                || (OR(7) && V(9) && !V(10) && !V(11) && !V(12)) 
                || (OR(8) && V(9) && !V(10) && !V(11) && !V(12)) 
                || (V(4) && OR(5) && V(9) && !V(10) && !V(11) && !V(12)) 
                || (OR(6) && !V(10) && !V(11) && V(12)) 
                || (OR(7) && !V(10) && !V(11) && V(12)) 
                || (OR(8) && !V(10) && !V(11) && V(12)) 
                || (V(4) && OR(5) && !V(10) && !V(11) && V(12)) 
                || (V(3) && !V(10) && !V(11)) || (V(1) && !V(10) && !V(11)) 
                || (V(2) && !V(10) && !V(11));
        }
    }
}
