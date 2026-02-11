using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class GEN_NKG2 : SheetLogic
    {
        public GEN_NKG2()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (V(2) && V(3) && V(4) && V(5) && OR(6) && V(7) && OR(8)) 
                || (V(2) && V(3) && V(4) && V(5) && !OR(6)) 
                || (V(1) && V(2) && V(3) && V(4) && OR(6) && V(7) && OR(8));
        }
    }
}
