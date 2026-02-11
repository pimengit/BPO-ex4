using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SIGNAL_SV275 : SheetLogic
    {
        public SIGNAL_SV275()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(300);
        }

        public override bool Compute()
        {
            return (OR(2)) || (V(1));
        }
    }
}
