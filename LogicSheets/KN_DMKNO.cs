using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KN_DMKNO : SheetLogic
    {
        public KN_DMKNO()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) && V(2) && V(3);
        }
    }
}
