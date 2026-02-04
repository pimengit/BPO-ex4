using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_DPS2 : SheetLogic
    {
        public SIGNAL_DPS2()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.Zero;
        }

        public override bool Compute()
        {
            return V(1) && V(2) && V(3) && V(4) && V(5);
        }
    }
}
