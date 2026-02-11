using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_UPN : SheetLogic
    {
        public SECT_UPN()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && V(2) && !V(4) && !V(6) && V(7)) || (!V(3) && !V(5) && V(6)) || (V(3));
        }
    }
}
