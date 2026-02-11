using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_zD45 : SheetLogic
    {
        public SECT_zD45()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!OR(1) && !OR(2) && V(3) && !V(4) && OR(8) && V(9)) 
                || (!OR(1) && !OR(2) && V(3) && !V(4) && OR(7) && V(9)) 
                || (!OR(1) && !OR(2) && V(3) && !V(4) && OR(6) && V(9)) 
                || (!OR(1) && !OR(2) && V(3) && !V(4) && V(5) && V(9)) 
                || (!V(4) && V(10));
        }
    }
}
