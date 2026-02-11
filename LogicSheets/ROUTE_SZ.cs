using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTE_SZ : SheetLogic
    {
        public ROUTE_SZ()
        {
            OnDelay = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (!OR(1) && !OR(2) && !OR(3));
        }
    }
}
