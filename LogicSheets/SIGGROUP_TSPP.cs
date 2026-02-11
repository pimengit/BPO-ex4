using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGGROUP_TSPP : SheetLogic
    {
        public SIGGROUP_TSPP()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && AND(2));
        }
    }
}
