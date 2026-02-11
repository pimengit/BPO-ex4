using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class STAGE_OTPR : SheetLogic
    {
        public STAGE_OTPR()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2) && V(3) && !V(4));
        }
    }
}
