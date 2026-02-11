using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SPEED_70s : SheetLogic
    {
        public SPEED_70s()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1)) || (V(2));
        }
    }
}
