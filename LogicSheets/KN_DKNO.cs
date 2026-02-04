using System;
using System.Reflection.Metadata.Ecma335;
using BPO_ex4.StationLogic;

namespace BPO_ex4.LogicSheets
{
    class KN_DKNO : SheetLogic
    {
        public KN_DKNO()
        {
            OnDelay  = TimeSpan.FromMicroseconds(1);
            OffDelay = TimeSpan.FromMilliseconds(50);
        }

        public override bool Compute()
        {
            return OR(1) || OR(2);
        }
    }
}
