using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class OKSE_L5_1 : SheetLogic
    {
        public OKSE_L5_1()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(4) && !V(5) && OR(6) && V(8)) || (V(1) && !V(4) && OR(7) && V(8)) || (V(1) && V(3) && !V(4) && V(8)) 
                || (V(2) && !V(4) && !V(5) && OR(6) && V(8)) || (V(2) && !V(4) && OR(7) && V(8)) || (V(2) && V(3) && !V(4) && V(8));
        }
    }
}
