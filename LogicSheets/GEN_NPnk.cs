using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_NPnk : SheetLogic
    {
        public GEN_NPnk()
        {
            OnDelay = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(4)) || (OR(2) && !V(5) && V(7)) || (OR(3) && !V(6) && V(7)) || (V(1));
        }
    }
}
