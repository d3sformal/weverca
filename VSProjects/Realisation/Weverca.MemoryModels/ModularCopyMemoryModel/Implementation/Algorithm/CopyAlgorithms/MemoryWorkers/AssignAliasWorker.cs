using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// Implementation of algorithm for creating aliases. Creates alias links between all memory
    /// locations determined by given source and target collectors.
    /// 
    /// Data of every must target index are removed and aliased data are copied into this location. 
    /// </summary>
    class AssignAliasWorker
    {
        private Snapshot snapshot;

        AssignWorker assignWorker;

        List<MemoryIndex> mustSource = new List<MemoryIndex>();
        List<MemoryIndex> maySource = new List<MemoryIndex>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignAliasWorker"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AssignAliasWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;

            assignWorker = new AssignWorker(snapshot);
        }

        /// <summary>
        /// Assigns the alias.
        /// </summary>
        /// <param name="sourceCollector">The source collector.</param>
        /// <param name="targetCollector">The target collector.</param>
        /// <param name="dataIndex">Index of the data.</param>
        internal void AssignAlias(IIndexCollector sourceCollector, IIndexCollector targetCollector, MemoryIndex dataIndex)
        {
            assignWorker.Assign(targetCollector, dataIndex);
            makeAliases(sourceCollector, targetCollector);
        }

        /// <summary>
        /// Makes the aliases.
        /// </summary>
        /// <param name="sourceCollector">The source collector.</param>
        /// <param name="targetCollector">The target collector.</param>
        private void makeAliases(IIndexCollector sourceCollector, IIndexCollector targetCollector)
        {
            //Must target
            foreach (MemoryIndex index in targetCollector.MustIndexes)
            {
                snapshot.MustSetAliases(index, sourceCollector.MustIndexes, sourceCollector.MayIndexes);
            }

            //Must source
            foreach (MemoryIndex index in sourceCollector.MustIndexes)
            {
                snapshot.AddAliases(index, targetCollector.MustIndexes, targetCollector.MayIndexes);
            }

            //May target
            HashSet<MemoryIndex> sourceAliases = new HashSet<MemoryIndex>(sourceCollector.MustIndexes.Concat(sourceCollector.MayIndexes));
            foreach (MemoryIndex index in targetCollector.MayIndexes)
            {
                snapshot.MaySetAliases(index, sourceAliases);
            }

            //May source
            HashSet<MemoryIndex> targetAliases = new HashSet<MemoryIndex>(targetCollector.MustIndexes.Concat(targetCollector.MayIndexes));
            foreach (MemoryIndex index in sourceCollector.MayIndexes)
            {
                snapshot.AddAliases(index, null, targetAliases);
            }
        }
    }
}
