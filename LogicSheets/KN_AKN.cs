using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KN_AKN : SheetLogic
    {
        public KN_AKN()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return OR(1) && V(2);
        }
    }
}
