using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_SMAR : SheetLogic
    {
        public AVTODO_SMAR()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //
        public override bool Compute()
        {
            return (!V(1) && V(2)) || (!V(1) && V(3));
        }
    }
}
