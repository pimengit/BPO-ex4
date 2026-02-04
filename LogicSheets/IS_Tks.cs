using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class IS_Tks : SheetLogic
    {
        public IS_Tks()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromSeconds(0.45);
        }

        public override bool Compute()
        {
            return V(1) && V(2);
        }
    }
}
