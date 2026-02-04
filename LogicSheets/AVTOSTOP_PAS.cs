using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTOSTOP_PAS : SheetLogic
    {
        public AVTOSTOP_PAS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromSeconds(4);
        }
        //
        public override bool Compute()
        {
            return V(1) && V(2);
        }
    }
}
