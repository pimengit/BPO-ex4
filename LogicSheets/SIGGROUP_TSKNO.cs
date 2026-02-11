using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGGROUP_TSKNO : SheetLogic
    {
        public SIGGROUP_TSKNO()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1)) || (OR(2));
        }
    }
}
