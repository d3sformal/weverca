using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class TransactionEntry
    {
        Stopwatch stopwatch;


        internal void Start()
        {
            stopwatch.Start();
        }

        internal void Stop()
        {
            throw new NotImplementedException();
        }

        internal long Duration()
        {
            throw new NotImplementedException();
        }
    }
}
