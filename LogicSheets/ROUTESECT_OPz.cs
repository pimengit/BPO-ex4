using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class ROUTESECT_OPz : SheetLogic
    {
        public ROUTESECT_OPz()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(1) && OR(2)) || (OR(1) && OR(3));
        }
    }
}
