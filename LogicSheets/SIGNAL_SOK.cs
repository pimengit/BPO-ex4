using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_SOK : SheetLogic
    {
        public SIGNAL_SOK()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!OR(1) && OR(2)) || (!OR(1) && V(3)) || (!OR(1) && V(4)) || (!OR(1) && V(5)) || (!OR(1) && AND(6));
        }
    }
}
