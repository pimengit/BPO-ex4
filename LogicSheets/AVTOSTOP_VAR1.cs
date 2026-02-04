using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTOSTOP_VAR1 : SheetLogic
    {
        public AVTOSTOP_VAR1()
        {
            OnDelay  = TimeSpan.Zero;
            OffDelay = TimeSpan.Zero;
        }
        //
        public override bool Compute()
        {
            return V(1) || !V(2) && V(3);
        }
    }
}
