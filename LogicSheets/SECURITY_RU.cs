using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECURITY_RU : SheetLogic
    {
        public SECURITY_RU()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !OR(3) && !V(4)) || (OR(2) && !OR(3) && !V(4));
        }
    }
}
