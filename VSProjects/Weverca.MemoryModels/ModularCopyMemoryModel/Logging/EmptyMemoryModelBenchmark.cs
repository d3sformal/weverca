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

        public void WriteResultsToFile(string benchmarkFile)
        {
        }

        public void ClearResults()
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




        public IEnumerable<TransactionEntry> TransactionResults
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyDictionary<AlgorithmType, AlgorithmAggregationEntry> AlgorithmResults
        {
            get { throw new NotImplementedException(); }
        }

        public double TotalOperationTime
        {
            get { throw new NotImplementedException(); }
        }

        public double TotalAlgorithmTime
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfOperations
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfAlgorithms
        {
            get { throw new NotImplementedException(); }
        }

        public int NumberOfTransactions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
