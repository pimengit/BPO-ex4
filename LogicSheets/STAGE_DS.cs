using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class STAGE_DS : SheetLogic
    {
        public STAGE_DS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(2) && !V(3) && V(4) && !OR(5) && !V(6) && V(7)) 
                || (V(1) && !V(3) && V(4) && !OR(5) && !V(6)) 
                || (V(4) && !OR(5) && V(6));
        }
    }
}
