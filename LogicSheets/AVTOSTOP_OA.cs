using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTOSTOP_OA : SheetLogic
    {
        public AVTOSTOP_OA()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //ë
        public override bool Compute()
        {
            return V(1) && V(2);
        }
    }
}
