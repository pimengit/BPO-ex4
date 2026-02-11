using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_ARTOP : SheetLogic
    {
        public AVTODO_ARTOP()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //k
        public override bool Compute()
        {
            return (V(1) && V(2));
        }
    }
}
