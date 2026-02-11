using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_SCAR : SheetLogic
    {
        public AVTODO_SCAR()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(1) && V(3)) || (!OR(1) && V(4)) || (!V(2) && V(4));
        }
    }
}
