using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_AOG1 : SheetLogic
    {
        public GEN_AOG1()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!V(1) && V(2) && V(3) && V(4) && !OR(5) && !V(6)) ||
                   (V(1) && V(2) && V(3) && !OR(5) && !V(6));
        }
    }
}
