using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class STAGE_CRM : SheetLogic
    {
        public STAGE_CRM()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(2)) || (V(1));
        }
    }
}
