using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SPEED_80U : SheetLogic
    {
        public SPEED_80U()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && !V(3) && V(4) && AND(5) && V(6) && AND(7) && AND(9) && V(10)) 
                || (!V(1) && !V(3) && V(4) && AND(5) && V(6) && AND(8) && AND(9) && V(10)) 
                || (V(1) && V(2) && !V(3) && V(4) && AND(5) && V(6) && AND(7) && AND(9) && V(10)) 
                || (V(1) && V(2) && !V(3) && V(4) && AND(5) && V(6) && AND(8) && AND(9) && V(10));
        }
    }
}
