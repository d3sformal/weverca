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
        public double Time { get; set; }
        public double MemoryStart { get; set; }
        public double MemoryStop { get; set; }
        public double Memory { get; set; }

        public AlgorithmEntry(AlgorithmType algorithmType)
        {
            this.algorithmType = algorithmType;
        }


    }
}
