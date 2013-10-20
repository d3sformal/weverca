using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class AssignWorker
    {
        private Snapshot snapshot;

        public AssignWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        internal void Assign(AssignCollector collector, MemoryEntry sourceEntry)
        {
            foreach (MemoryIndex mustIndex in collector.MustIndexes)
            {
                snapshot.SetMemoryEntry(mustIndex, sourceEntry);
            }

            foreach (MemoryIndex mayIndex in collector.MayIndexes)
            {
                MemoryEntry oldEntry = snapshot.GetMemoryEntry(mayIndex);

                HashSet<Value> values = new HashSet<Value>();
                foreach (Value value in oldEntry.PossibleValues)
                {
                    values.Add(value);
                }

                foreach (Value value in sourceEntry.PossibleValues)
                {
                    values.Add(value);
                }

                MemoryEntry newEntry = new MemoryEntry(values);
                snapshot.SetMemoryEntry(mayIndex, newEntry);
            }
        }
    }
}
