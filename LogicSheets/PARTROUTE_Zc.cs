using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class PARTROUTE_Zc : SheetLogic
    {
        public PARTROUTE_Zc()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && V(2) && AND(3) && V(4)) || (!V(1) && V(5)) || (!V(1) && V(6));
        }
    }
}
