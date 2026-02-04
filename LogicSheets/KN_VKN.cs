using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KN_VKN : SheetLogic
    {
        public KN_VKN()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return !OR(1) && !OR(2);
        }
    }
}
