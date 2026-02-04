using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KOD_PNT : SheetLogic
    {
        public KOD_PNT()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) || OR(2) || OR(3) || OR(4);
        }
    }
}
