using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KOD_PNT : SheetLogic
    {
        public KOD_PNT()
        {
            OnDelay  = TimeSpan.FromMilliseconds(600);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(3)) || (OR(2)) || (OR(4)) || (V(1));
        }
    }
}
