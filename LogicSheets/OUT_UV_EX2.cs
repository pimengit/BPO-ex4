using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class OUT_UV_EX2 : SheetLogic
    {
        public OUT_UV_EX2()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return V(1);
        }
    }
}
