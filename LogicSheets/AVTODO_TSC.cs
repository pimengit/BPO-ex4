using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_TSC : SheetLogic
    {
        public AVTODO_TSC()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }//

        public override bool Compute()
        {
            return (V(1) && !V(2) && V(4)) 
                || (V(1) && !V(2) && V(5)) 
                || (V(1) && !V(2) && V(6)) 
                || (V(1) && !V(3) && V(4)) 
                || (V(1) && !V(3) && V(5)) 
                || (V(1) && !V(3) && V(6));
        }
    }
}
