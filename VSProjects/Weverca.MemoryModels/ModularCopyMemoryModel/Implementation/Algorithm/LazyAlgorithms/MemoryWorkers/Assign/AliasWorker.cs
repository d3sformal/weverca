using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    /// <summary>
    /// Provides assign operation on source indexes for aliases. Missing indexes has to be created 
    /// but existing cannot be modified. Produces MemoryEntryCollector which contains merged source data
    /// which will be assigned to target indexes.
    /// </summary>
    class AliasWorker : AbstractAssignWorker
    {
        /// <summary>
        /// Gets the entry collector.
        /// </summary>
        /// <value>
        /// The entry collector.
        /// </value>
        public MemoryEntryCollector EntryCollector { get; private set; }

        private HashSet<MemoryIndex> mustAliases = new HashSet<MemoryIndex>();
        private HashSet<MemoryIndex> mayAliases = new HashSet<MemoryIndex>();
        private HashSet<Value> values = new HashSet<Value>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasWorker"/> class.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="treeCollector">The tree collector.</param>
        /// <param name="pathModifications">The path modifications.</param>
        public AliasWorker(ModularMemoryModelFactories factories, Snapshot snapshot, TreeIndexCollector treeCollector, MemoryIndexModificationList pathModifications)
            : base(factories, snapshot, treeCollector, pathModifications)
        {
            EntryCollector = new MemoryEntryCollector(snapshot);
        }

        /// <summary>
        /// Collects the aliases.
        /// </summary>
        public void CollectAliases()
        {
            ProcessCollector();

            EntryCollector.ProcessRootIndexes(mustAliases, mayAliases, values);
        }


        protected override void collectValueNode(ValueCollectorNode node)
        {

        }

        protected override void collectMemoryIndexCollectorNode(MemoryIndexCollectorNode node)
        {
            storeAliasIndex(node);
            continueMemoryIndexCollectorNode(node);
        }

        protected override void collectUnknownIndexCollectorNode(UnknownIndexCollectorNode node)
        {
            storeAliasIndex(node);
            continueUnknownIndexCollectorNode(node);
        }

        protected override void collectUndefinedCollectorNode(UndefinedCollectorNode node)
        {
            storeAliasIndex(node);
            continueUndefinedCollectorNode(node);
        }

        private void storeAliasIndex(LocationCollectorNode node)
        {
            if (node.IsMust)
            {
                mustAliases.Add(node.TargetIndex);
            }
            else
            {
                mayAliases.Add(node.TargetIndex);
            }
        }
    }
}
