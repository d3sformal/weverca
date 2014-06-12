using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    class AlgorithmEntry
    {
        private AlgorithmType algorithmType;

        public int Starts { get; set; }
        public int Stops { get; set; }
        public long Time { get; set; }

        public AlgorithmEntry(AlgorithmType algorithmType)
        {
            this.algorithmType = algorithmType;
        }

    }
}
