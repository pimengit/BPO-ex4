using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_AR : SheetLogic
    {
        public AVTODO_AR()
        {
            OnDelay = TimeSpan.Zero;
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //уточнить
        public override bool Compute()
        {
            return (V(2) && !V(3) && !OR(5) && V(7) && AND(8) && !OR(9) && !V(10) && !OR(11) && !V(12) && V(14)) ||
                   (OR(4) && !OR(5) && AND(8) && !OR(9) && !V(10) && !OR(11) && V(12)) ||
                   (OR(5) && V(6) && AND(8) && !OR(9) && !V(10)) ||
                   (!V(3) && !OR(5) && V(7) && AND(8) && !OR(9) && !V(10) && !OR(11) && !V(12) && V(13)) ||
                   (V(1) && AND(8) && !OR(9) && !V(10) && !OR(11));
        }
    }
}
