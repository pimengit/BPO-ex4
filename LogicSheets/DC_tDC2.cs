using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class DC_tDC2 : SheetLogic
    {
        public DC_tDC2()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(750);
        }

        public override bool Compute()
        {
            return (V(1) && V(2));
        }
    }
}
