using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.MemoryModel
{
    class TransactionCounter
    {
        MemorySnapshot lastChangeIn;
        uint changeCounter;

        public TransactionCounter(MemorySnapshot snapshot)
        {
            lastChangeIn = snapshot;
            changeCounter = snapshot.TransactionCounter;
        }

        public bool Equals(TransactionCounter counter)
        {
            return lastChangeIn == counter.lastChangeIn && changeCounter == counter.changeCounter;
        }
    }
}
