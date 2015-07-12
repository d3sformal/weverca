using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    public class EmptyMemoryModelBenchmark : IBenchmark
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

        public void StartAlgorithm(Snapshot snapshot, Interfaces.Algorithm.IAlgorithm algorithmInstance, AlgorithmType algorithmType)
        {
        }

        public void FinishAlgorithm(Snapshot snapshot, Interfaces.Algorithm.IAlgorithm algorithmInstance, AlgorithmType algorithmType)
        {
        }

        public void WriteResultsToFile(string benchmarkFile)
        {
        }

        public IReadOnlyDictionary<AlgorithmType, AlgorithmEntry> AlgorithmResults
        {
            get { throw new NotImplementedException(); }
        }

        public int TransactionStarts
        {
            get { throw new NotImplementedException(); }
        }

        public int TransactionStops
        {
            get { throw new NotImplementedException(); }
        }

        public double TransactionTime
        {
            get { throw new NotImplementedException(); }
        }

        public int AlgorithmStarts
        {
            get { throw new NotImplementedException(); }
        }

        public int AlgorithmStops
        {
            get { throw new NotImplementedException(); }
        }

        public double AlgorithmTime
        {
            get { throw new NotImplementedException(); }
        }

        public int Initializations
        {
            get { throw new NotImplementedException(); }
        }

        public double TransactionMemory
        {
            get { throw new NotImplementedException(); }
        }

        public double AlgorithmMemory
        {
            get { throw new NotImplementedException(); }
        }

        public void ClearResults()
        {
        }
    }
}
