using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_zD125 : SheetLogic
    {
        public SECT_zD125()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!OR(1) && !OR(2) && V(4) && !V(5) && OR(9) && AND(10)) 
                || (!OR(1) && !OR(2) && V(4) && !V(5) && OR(8) && AND(10)) 
                || (!OR(1) && !OR(2) && V(4) && !V(5) && V(7) && AND(10)) 
                || (!OR(1) && !OR(2) && V(4) && !V(5) && V(6) && AND(10)) 
                || (!V(5) && V(11)) 
                || (!V(3) && !V(5));
        }
    }
}
