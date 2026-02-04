using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KGU_KKGU : SheetLogic
    {
        public KGU_KKGU()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) || V(2);
        }
    }
}
