using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KOD_tOD : SheetLogic
    {
        public KOD_tOD()
        {
            OnDelay  = TimeSpan.FromMilliseconds(10000);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && V(2) && !V(3) && !V(4));
        }
    }
}
