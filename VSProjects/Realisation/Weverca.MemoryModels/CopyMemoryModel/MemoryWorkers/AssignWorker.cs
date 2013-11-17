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

        internal void Assign(IIndexCollector collector, MemoryIndex sourceIndex)
        {
            foreach (MemoryIndex mustIndex in collector.MustIndexes)
            {
                snapshot.DestroyMemory(mustIndex);
                CopyWithinSnapshotWorker copyWorker = new CopyWithinSnapshotWorker(snapshot, true);
                copyWorker.Copy(sourceIndex, mustIndex);
            }

            foreach (MemoryIndex mayIndex in collector.MayIndexes)
            {
                MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
                mergeWorker.MergeIndexes(mayIndex, sourceIndex);
            }
        }
    }
}
