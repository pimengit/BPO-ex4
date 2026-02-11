using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class DC_CAN : SheetLogic
    {
        public DC_CAN()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(4)) || (V(1) && V(2) && V(3));
        }
    }
}
