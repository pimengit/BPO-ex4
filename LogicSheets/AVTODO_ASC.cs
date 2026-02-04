using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_ASC : SheetLogic
    {
        public AVTODO_ASC()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //уточнить
        public override bool Compute()
        {
            return (!V(1) && V(2) && V(3) && V(4) && OR(11) && V(12)) ||
                   (V(1) && V(2) && V(3) && V(4) && !OR(11) && V(12)) ||
                   (V(2) && V(3) && V(13)) ||
                   (V(2) && V(3) && !V(5) && V(6)) ||
                   (V(2) && V(3) && !V(5) && V(7) && V(8)) ||
                   (V(2) && V(3) && !V(5) && V(9) && V(10));
        }
    }
}
