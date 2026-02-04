using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_OZMS : SheetLogic
    {
        public SIGNAL_OZMS()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) && V(2) && V(3) && V(4) && V(5) && V(6);
        }
    }
}
