using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_OAR : SheetLogic
    {
        public AVTODO_OAR()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //
        public override bool Compute()
        {
            return !V(4) && V(5) && V(6) || V(3) && V(5) || V(1) && !V(2);
        }
    }
}
