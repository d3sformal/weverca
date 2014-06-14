using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class Measuring
    {
        public Stopwatch Stopwatch { get; private set; }
        public double MemoryStart { get; set; }

        public Measuring()
        {
            Stopwatch = new Stopwatch();
        }
    }
}
