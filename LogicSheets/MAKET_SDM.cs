using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class MAKET_SDM : SheetLogic
    {
        public MAKET_SDM()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && V(2) && !OR(4)) || (V(1) && V(3) && !OR(4));
        }
    }
}
