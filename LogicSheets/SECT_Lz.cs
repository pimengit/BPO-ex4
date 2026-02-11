using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_Lz : SheetLogic
    {
        public SECT_Lz()
        {
            OnDelay  = TimeSpan.FromMilliseconds(150);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && !OR(2)) || (!V(1) && V(3));
        }
    }
}
