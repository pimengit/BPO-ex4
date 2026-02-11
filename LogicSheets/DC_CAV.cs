using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class DC_CAV : SheetLogic
    {
        public DC_CAV()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(7)) || (V(4) && V(5) && V(6)) || (OR(3)) || (OR(2)) || (OR(1));
        }
    }
}
