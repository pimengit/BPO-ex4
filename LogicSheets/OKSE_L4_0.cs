using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class OKSE_L4_0 : SheetLogic
    {
        public OKSE_L4_0()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(3) && OR(4) && V(5)) || (V(2) && !V(3) && OR(4) && V(5));
        }
    }
}
