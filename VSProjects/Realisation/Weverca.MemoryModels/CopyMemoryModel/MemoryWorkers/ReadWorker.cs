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
    class ReadWorker
    {
        private Snapshot snapshot;


        public ReadWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public MemoryEntry ReadValue(IIndexCollector collector)
        {
            if (collector.MustIndexesCount == 1 && collector.IsDefined)
            {
                MemoryIndex index = collector.MustIndexes.First();
                return snapshot.GetMemoryEntry(index);
            }
            else
            {
                HashSet<Value> values = new HashSet<Value>();
                if (!collector.IsDefined)
                {
                    values.Add(snapshot.UndefinedValue);
                }

                foreach (MemoryIndex index in collector.MustIndexes)
                {
                    MemoryEntry entry = snapshot.GetMemoryEntry(index);
                    foreach (Value value in entry.PossibleValues)
                    {
                        values.Add(value);
                    }
                }

                return new MemoryEntry(values);
            }

        }
    }
}
