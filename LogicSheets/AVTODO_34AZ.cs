using System;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class AVTODO_34AZ : SheetLogic
    {
        public AVTODO_34AZ()
        {
            OnDelay  = TimeSpan.FromMilliseconds(50);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return (OR(1) && OR(2) && !V(3)) || (OR(1) && !V(3) && V(4));
            
        }
    }
}
