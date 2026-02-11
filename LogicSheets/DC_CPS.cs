using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class DC_CPS : SheetLogic
    {
        public DC_CPS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(5)) || (OR(1)) || (V(2) && V(3) && V(4));
        }
    }
}
