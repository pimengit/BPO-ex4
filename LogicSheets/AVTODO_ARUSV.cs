using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_ARUSV : SheetLogic
    {
        public AVTODO_ARUSV()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //уточнить
        public override bool Compute()
        {
            return (V(1) && !V(2) && !OR(3) && V(4) && AND(5) && !OR(6) && !V(7) && !OR(8) && !V(9));
        }
    }
}
