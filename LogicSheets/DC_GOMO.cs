using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class DC_GOMO : SheetLogic
    {
        public DC_GOMO()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(2) && V(4)) || (V(2) && V(3)) || (V(1));
        }
    }
}
