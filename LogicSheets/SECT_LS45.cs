using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_LS45 : SheetLogic
    {
        public SECT_LS45()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && V(3) && V(4) && OR(5) && !V(6) && OR(7) && !V(8)) 
                || (!V(1) && V(3) && V(4) && OR(5) && !V(6) && OR(9) && !V(10)) 
                || (!V(1) && V(3) && V(4) && OR(7) && !V(8) && OR(9) && !V(10)) 
                || (!V(1) && !V(2) && V(3) && V(4) && !OR(5) && !V(6) && !OR(7) && !V(8) && !OR(9) && !V(10));
        }
    }
}
