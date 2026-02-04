using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_ARUS : SheetLogic
    {
        public AVTODO_ARUS()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }
        //уточнить
        public override bool Compute()
        {
            return (!OR(1) && V(2)) ||
                   (!OR(1) && V(3)) ||
                   V(4);
        }
    }
}
