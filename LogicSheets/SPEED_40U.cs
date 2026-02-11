using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SPEED_40U : SheetLogic
    {
        public SPEED_40U()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && V(3) && AND(4) && V(5) && AND(6) && AND(8) && V(9)) 
                || (!V(1) && V(3) && AND(4) && V(5) && AND(7) && AND(8) && V(9)) 
                || (V(1) && V(2) && V(3) && AND(4) && V(5) && AND(6) && AND(8) && V(9)) 
                || (V(1) && V(2) && V(3) && AND(4) && V(5) && AND(7) && AND(8) && V(9));
        }
    }
}
