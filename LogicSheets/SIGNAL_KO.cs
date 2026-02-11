using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_KO : SheetLogic
    {
        public SIGNAL_KO()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (AND(1) && V(5) && !V(6)) || (AND(1) && V(5) && V(6)) || (!V(2) && V(3) && !V(4)) || (V(2) && !V(4));
        }
    }
}
