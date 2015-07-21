using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Logging
{
    public class AlgorithmAggregationEntry
    {
        public AlgorithmType AlgorithmType { get; private set; }

        public int NumberOfRuns { get; private set; }

        public double TotalTime { get; private set; }

        public IEnumerable<AlgorithmEntry> Results { get { return results; } }

        private List<AlgorithmEntry> results = new List<AlgorithmEntry>();

        public AlgorithmAggregationEntry(AlgorithmType algorithmType)
        {
            AlgorithmType = algorithmType;

            NumberOfRuns = 0;
            TotalTime = 0;
        }

        public void AlgorithmStopped(AlgorithmEntry entry) 
        {
            NumberOfRuns++;
            TotalTime += entry.AlgorithmTime;

            results.Add(entry);
        }
    }
}
