using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    /// <summary>
    /// Implementation of benchmark which gives no output.
    /// 
    /// This is the default benchmark.
    /// </summary>
    class EmptyMemoryModelBenchmark : IBenchmark
    {
        public void InitializeSnapshot(Snapshot snapshot)
        {
        }

        public void StartTransaction(Snapshot snapshot)
        {
        }

        public void FinishTransaction(Snapshot snapshot)
        {
        }

        public void StartAlgorithm(Snapshot snapshot, AlgorithmType algorithmType)
        {
        }

        public void FinishAlgorithm(Snapshot snapshot, AlgorithmType algorithmType)
        {
        }

        public void StartOperation(Snapshot snapshot)
        {
        }

        public void FinishOperation(Snapshot snapshot)
        {
        }
    }
}
