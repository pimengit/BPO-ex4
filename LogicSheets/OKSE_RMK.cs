using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class OKSE_RMK : SheetLogic
    {
        public OKSE_RMK()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(15000);
        }

        public override bool Compute()
        {
            return (V(1) && V(2) && !V(3));
        }
    }
}
