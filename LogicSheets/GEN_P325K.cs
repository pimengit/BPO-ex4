using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_P325K : SheetLogic
    {
        public GEN_P325K()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(5) && V(6)) ||
                   (V(7) && V(8)) ||
                   (V(1) && V(3) && V(10)) ||
                   (V(2) && V(4) && V(9));
        }
    }
}
