using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_OSC : SheetLogic
    {
        public AVTODO_OSC()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(1) && OR(2) && !OR(3) && !OR(4));
        }
    }
}
