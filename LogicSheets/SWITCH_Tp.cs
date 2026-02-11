using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SWITCH_Tp : SheetLogic
    {
        public SWITCH_Tp()
        {
            OnDelay  = TimeSpan.FromMilliseconds(1500);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(2) && !V(3)) || (V(4)) || (V(5)) || (V(6)) || (V(7)) || (V(1));
        }
    }
}
