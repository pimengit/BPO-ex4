using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGGROUP_KP1 : SheetLogic
    {
        public SIGGROUP_KP1()
        {
            OnDelay  = TimeSpan.FromMilliseconds(7000);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && !V(2) && V(3) && V(4) && !V(5));
        }
    }
}
