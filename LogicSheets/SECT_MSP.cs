using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SECT_MSP : SheetLogic
    {
        public SECT_MSP()
        {
            OnDelay = TimeSpan.FromSeconds(1.5);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1) && V(2);
        }
    }
}
