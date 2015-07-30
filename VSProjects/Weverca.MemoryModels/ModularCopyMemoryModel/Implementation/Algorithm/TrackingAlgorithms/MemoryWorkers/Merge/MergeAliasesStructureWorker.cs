using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    /// <summary>
    /// Provides merge operations connected with aliases
    /// 
    /// Collects all aliases from source indexes and merges the to the target.
    /// </summary>
    class MergeAliasStructureWorker
    {
        private IWriteableSnapshotStructure writeableTargetStructure;
        private TrackingMergeStructureWorker worker;

        private ReferenceCollector references;
        private bool hasAliases = false;
        private bool aliasesAlwaysDefined = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeAliasStructureWorker"/> class.
        /// </summary>
        /// <param name="writeableTargetStructure">The writeable target structure.</param>
        /// <param name="worker">The worker.</param>
        public MergeAliasStructureWorker(IWriteableSnapshotStructure writeableTargetStructure, TrackingMergeStructureWorker worker)
        {
            this.writeableTargetStructure = writeableTargetStructure;
            this.worker = worker;

            references = new ReferenceCollector(writeableTargetStructure);
        }

        /// <summary>
        /// Collects the source aliases.
        /// </summary>
        /// <param name="sourceAliases">The source aliases.</param>
        public void collectSourceAliases(IMemoryAlias sourceAliases)
        {
            // Collect source aliases
            if (sourceAliases != null && sourceAliases.HasAliases)
            {
                references.CollectMust(sourceAliases.MustAliases);
                references.CollectMay(sourceAliases.MayAliases);

                hasAliases = true;
            }
            else
            {
                aliasesAlwaysDefined = false;
            }
        }

        /// <summary>
        /// Merges the aliases to target and clear inner container.
        /// </summary>
        /// <param name="targetIndex">Index of the target.</param>
        /// <param name="operation">The operation.</param>
        /// <exception cref="System.Exception">Alias merge - memory index was not included into collection of aliases</exception>
        public void MergeAliasesAndClear(MemoryIndex targetIndex, MergeOperation operation)
        {
            if (hasAliases && references.HasAliases)
            {
                references.SetAliases(targetIndex, worker, aliasesAlwaysDefined && !operation.IsUndefined);

                MemoryAliasInfo aliasInfo;
                if (worker.MemoryAliases.TryGetValue(targetIndex, out aliasInfo))
                {
                    aliasInfo.IsTargetOfMerge = true;
                }
                else
                {
                    throw new Exception("Alias merge - memory index was not included into collection of aliases");
                }

                references.Clear();
                hasAliases = false;
            }
            aliasesAlwaysDefined = true;
        }
    }

}
