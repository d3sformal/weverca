using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class AssignAliasWorker
    {
        private Snapshot snapshot;

        AssignWorker assignWorker;

        List<MemoryIndex> mustSource = new List<MemoryIndex>();
        List<MemoryIndex> maySource = new List<MemoryIndex>();

        public AssignAliasWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;

            assignWorker = new AssignWorker(snapshot);
        }

        internal void AssignAlias(IIndexCollector collector, AliasData data)
        {
            //filterData(data);
            assignWorker.Assign(collector, data.SourceIndex);
            makeAliases(collector, data);
        }

        private void filterData(AliasData data)
        {
            foreach (MemoryIndex mustAlias in data.MustIndexes)
            {
                filterAlias(mustAlias, mustSource, maySource);
            }

            foreach (MemoryIndex mustAlias in data.MayIndexes)
            {
                filterAlias(mustAlias, maySource, maySource);
            }
        }

        private void filterAlias(MemoryIndex alias, List<MemoryIndex> mustSource, List<MemoryIndex> maySource)
        {
            MemoryEntry entry;
            if (snapshot.Structure.TryGetMemoryEntry(alias, out entry))
            {
                if (entry.ContainsUndefinedValue)
                {
                    if (entry.Count > 1)
                    {
                        // Entry contains undefined value and something else
                        // Report MAY undefined alias and MAY set alias

                        maySource.Add(alias);
                    }
                    else
                    {
                        // entry contains only undefined value
                        // report MUST undefined alias
                    }
                }
                else if (entry.Count == 0)
                {
                    // There is no data in memory entry
                    // report undefined alias
                }
                else
                {
                    // There is some data without undefined value in memory entry
                    // Create must alias

                    mustSource.Add(alias);
                }
            }
            else
            {
                // There is no entry at all - index is not defined
                // report undefined alias
            }
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
