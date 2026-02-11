using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SPEED_P60 : SheetLogic
    {
        public SPEED_P60()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && AND(2) && !V(3) && !V(4));
        }
    }
}
