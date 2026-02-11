using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class DC_tDC : SheetLogic
    {
        public DC_tDC()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(5000);
        }

        public override bool Compute()
        {
            return (V(9)) || (V(8)) || (V(7)) || (OR(6)) || (V(5)) || (OR(4)) || (OR(3)) || (V(1)) || (OR(2));
        }
    }
}
