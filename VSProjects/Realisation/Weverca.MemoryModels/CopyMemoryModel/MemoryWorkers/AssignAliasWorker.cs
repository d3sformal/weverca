using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class AssignAliasWorker
    {
        private Snapshot snapshot;

        AssignWorker assignWorker;

        public AssignAliasWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;

            assignWorker = new AssignWorker(snapshot);
        }

        internal void AssignAlias(IIndexCollector collector, AliasData data)
        {
            assignWorker.Assign(collector, data.SourceIndex);
            makeAliases(collector, data);
        }

        private void makeAliases(IIndexCollector collector, AliasData data)
        {
            //Must target
            foreach (MemoryIndex index in collector.MustIndexes)
            {
                snapshot.MustSetAliases(index, data.MustIndexes, data.MayIndexes);
            }

            //Must source
            foreach (MemoryIndex index in data.MustIndexes)
            {
                snapshot.AddAliases(index, collector.MustIndexes, collector.MayIndexes);
            }

            //May target
            HashSet<MemoryIndex> sourceAliases = new HashSet<MemoryIndex>(data.MustIndexes.Concat(data.MayIndexes));
            foreach (MemoryIndex index in collector.MayIndexes)
            {
                snapshot.MaySetAliases(index, sourceAliases);
            }

            //May source
            HashSet<MemoryIndex> targetAliases = new HashSet<MemoryIndex>(collector.MustIndexes.Concat(collector.MayIndexes));
            foreach (MemoryIndex index in data.MayIndexes)
            {
                snapshot.AddAliases(index, null, targetAliases);
            }
        }
    }
}
