using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_SLS : SheetLogic
    {
        public SECT_SLS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(3) && V(4) && !V(5)) || (AND(1) && V(6)) || (AND(1) && V(7)) || (OR(2));
        }
    }
}
