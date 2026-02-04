using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class OKSE_OKRK : SheetLogic
    {
        public OKSE_OKRK()
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
