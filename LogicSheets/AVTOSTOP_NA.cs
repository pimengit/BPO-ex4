using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTOSTOP_NA : SheetLogic
    {
        public AVTOSTOP_NA()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromSeconds(0.3);
        }
        //
        public override bool Compute()
        {
            return V(1) ||
                   (V(2) && !V(3) && !V(4) && V(5)) ||
                   (!V(2) && V(3) && V(4) && !V(5));
        }
    }
}
