using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class MAKET_t2M : SheetLogic
    {
        public MAKET_t2M()
        {
            OnDelay  = TimeSpan.FromMilliseconds(10000);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2) && V(4) && !OR(5) && V(6) && !V(8)) || (V(1) && !V(3) && V(4) && !OR(5) && V(7) && !V(8)) || (V(1) && V(4) && OR(5) && !V(8));
        }
    }
}
