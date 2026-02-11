using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class RELAY_KRK1 : SheetLogic
    {
        public RELAY_KRK1()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(1) && !V(2));
        }
    }
}
