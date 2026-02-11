using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class STAGE_ACHK : SheetLogic
    {
        public STAGE_ACHK()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (AND(1) && AND(2) && V(3) && !V(4) && !V(5) && !V(6) && V(8) && V(10)) || (AND(1) && AND(2) && V(4) && !V(7) && V(9));
        }
    }
}
