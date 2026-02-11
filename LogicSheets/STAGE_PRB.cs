using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class STAGE_PRB : SheetLogic
    {
        public STAGE_PRB()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(4) && OR(5)) || (!V(3) && V(4)) || (V(1) && !AND(2) && V(4));
        }
    }
}
