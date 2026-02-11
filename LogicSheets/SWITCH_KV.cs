using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SWITCH_KV : SheetLogic
    {
        public SWITCH_KV()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(3) && !V(4) && !V(6)) || (!V(5) && V(6)) || (V(2) && !V(5)) || (V(1) && !V(6));
        }
    }
}
