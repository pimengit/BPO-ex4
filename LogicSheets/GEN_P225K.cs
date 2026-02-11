using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_P225K : SheetLogic
    {
        public GEN_P225K()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(3) && V(7) && OR(8)) 
                || (!V(4) && V(10) && OR(11)) 
                || (V(1) && V(5) && V(14)) 
                || (V(2) && V(6) && V(13)) 
                || (V(3) && V(7) && OR(9)) 
                || (V(4) && V(10) && OR(12));
        }
    }
}
