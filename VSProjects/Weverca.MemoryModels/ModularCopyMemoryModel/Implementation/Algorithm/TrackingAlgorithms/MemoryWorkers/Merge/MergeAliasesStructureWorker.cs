using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{

    class MergeAliasStructureWorker
    {

        private IWriteableSnapshotStructure writeableTargetStructure;
        private TrackingMergeStructureWorker worker;

        private ReferenceCollector references;
        private bool hasAliases = false;
        private bool aliasesAlwaysDefined = true;
        
        public MergeAliasStructureWorker(IWriteableSnapshotStructure writeableTargetStructure, TrackingMergeStructureWorker worker)
        {
            this.writeableTargetStructure = writeableTargetStructure;
            this.worker = worker;

            references = new ReferenceCollector(writeableTargetStructure);
        }

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
