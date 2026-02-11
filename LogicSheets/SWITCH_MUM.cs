using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class SWITCH_MUM : SheetLogic
    {
        public SWITCH_MUM()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(2)) || (!OR(1) && OR(3) && AND(4));
        }
    }
}
