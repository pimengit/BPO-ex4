using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_OAR : SheetLogic
    {
        public AVTODO_OAR()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromSeconds(0.6);
        }
        //
        public override bool Compute()
        {
            return (V(1) && V(3)) ||
                   (!V(2) && V(3) && V(4));
        }
    }
}
