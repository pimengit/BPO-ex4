using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTOSTOP_VARCH : SheetLogic
    {
        public AVTOSTOP_VARCH()
        {
            OnDelay  = TimeSpan.FromMilliseconds(20);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //
        public override bool Compute()
        {//set AVTOSTOP_VAR[1] 0
            return V(1) && !V(2) || !V(1) && !V(3);
        }
    }
}
