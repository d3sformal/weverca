using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    class TrackingMergeStructureWorker : AbstractTrackingMergeWorker, IReferenceHolder
    {
        private IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotStructure> commonAncestor = null;
        internal Dictionary<MemoryIndex, MemoryAliasInfo> MemoryAliases { get; private set; }

        public TrackingMergeStructureWorker(Snapshot targetSnapshot, List<Snapshot> sourceSnapshots, bool isCallMerge = false)
            : base(targetSnapshot, sourceSnapshots, isCallMerge)
        {
            MemoryAliases = new Dictionary<MemoryIndex, MemoryAliasInfo>();
        }

        public void MergeStructure()
        {
            isStructureWriteable = true;

            createSnapshotContexts();
            collectStructureChanges();
            selectParentSnapshot();
            createNewStructure();
            
            mergeObjectDefinitions();
            mergeMemoryStacksRoots();

            processMergeOperations();

            updateAliases();
        }

        private void collectStructureChanges()
        {
            IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotStructure> ancestor = snapshotContexts[0].SourceStructure.IndexChangeTracker;

            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            changes.Add(snapshotContexts[0].ChangedIndexesTree);
            for (int x = 1; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];
                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                ancestor = getFirstCommonAncestor(context.SourceStructure.IndexChangeTracker, ancestor, currentChanges, changes);
            }

            commonAncestor = ancestor;
        }

        private void createNewStructure()
        {
            Structure = Snapshot.SnapshotStructureFactory.CopyInstance(targetSnapshot, parentSnapshotContext.SourceSnapshot.Structure);
            writeableTargetStructure = Structure.Writeable;
            targetStructure = writeableTargetStructure;

            writeableTargetStructure.ReinitializeIndexTracker(commonAncestor.Container);
        }

        private void updateAliases()
        {
            foreach (var item in MemoryAliases)
            {
                MemoryIndex targetIndex = item.Key;
                MemoryAliasInfo aliasInfo = item.Value;

                if (aliasInfo.IsTargetOfMerge)
                {
                    writeableTargetStructure.SetAlias(targetIndex, aliasInfo.Aliases.Build());
                }
                else
                {
                    /*IMemoryAlias currentAliases;
                    if (writeableTargetStructure.TryGetAliases(targetIndex, out currentAliases))
                    {
                        aliasInfo.Aliases.MayAliases.AddAll(currentAliases.MayAliases);
                        aliasInfo.Aliases.MustAliases.AddAll(currentAliases.MustAliases);
                    }
                    else
                    {
                        writeableTargetStructure.SetAlias(targetIndex, aliasInfo.Aliases.Build());
                    }*/
                }
            }
        }

        protected override TrackingMergeWorkerOperationAccessor createNewOperationAccessor(MergeOperation operation)
        {
            return new OperationAccessor(operation, targetSnapshot, this);
        }

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAliases">The must aliases.</param>
        /// <param name="mayAliases">The may aliases.</param>
        public void AddAliases(MemoryIndex index, IEnumerable<MemoryIndex> mustAliases, IEnumerable<MemoryIndex> mayAliases)
        {
            MemoryAliasInfo aliasInfo;
            IMemoryAliasBuilder alias;
            if (!MemoryAliases.TryGetValue(index, out aliasInfo))
            {
                alias = Structure.CreateMemoryAlias(index).Builder();
                aliasInfo = new MemoryAliasInfo(alias, false);

                MemoryAliases[index] = aliasInfo;
            }
            else
            {
                alias = aliasInfo.Aliases;
            }

            if (mustAliases != null)
            {
                alias.MustAliases.AddAll(mustAliases);
            }
            if (mayAliases != null)
            {
                alias.MayAliases.AddAll(mayAliases);
            }

            foreach (MemoryIndex mustIndex in alias.MustAliases)
            {
                if (alias.MayAliases.Contains(mustIndex))
                {
                    alias.MayAliases.Remove(mustIndex);
                }
            }
        }

        /// <summary>
        /// Adds the aliases to given index. Alias entry of the given alias indexes are not changed.
        /// If given memory index contains no aliases new alias entry is created.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="mustAlias">The must alias.</param>
        /// <param name="mayAlias">The may alias.</param>
        public void AddAlias(MemoryIndex index, MemoryIndex mustAlias, MemoryIndex mayAlias)
        {
            MemoryAliasInfo aliasInfo;
            IMemoryAliasBuilder alias;
            if (!MemoryAliases.TryGetValue(index, out aliasInfo))
            {
                alias = Structure.CreateMemoryAlias(index).Builder();
                aliasInfo = new MemoryAliasInfo(alias, false);

                MemoryAliases[index] = aliasInfo;
            }
            else
            {
                alias = aliasInfo.Aliases;
            }

            if (mustAlias != null)
            {
                alias.MustAliases.Add(mustAlias);

                if (alias.MayAliases.Contains(mustAlias))
                {
                    alias.MayAliases.Remove(mustAlias);
                }
            }

            if (mayAlias != null && !alias.MustAliases.Contains(mayAlias))
            {
                alias.MayAliases.Add(mayAlias);
            }
        }

        internal class MemoryAliasInfo
        {
            public IMemoryAliasBuilder Aliases { get; set; }
            public bool IsTargetOfMerge { get; set; }

            public MemoryAliasInfo(IMemoryAliasBuilder aliases, bool isTargetOfMerge)
            {
                this.Aliases = aliases;
                this.IsTargetOfMerge = isTargetOfMerge;
            }
        }

        internal class OperationAccessor : TrackingMergeWorkerOperationAccessor
        {

            private ReferenceCollector references = new ReferenceCollector();

            private MergeOperation operation;

            private Snapshot targetSnapshot;
            private TrackingMergeStructureWorker mergeWorker;

            private bool hasAliases = false;
            private bool hasAliasesAlways = true;

            public OperationAccessor(MergeOperation operation, Snapshot targetSnapshot, TrackingMergeStructureWorker mergeWorker)
            {
                this.operation = operation;
                this.targetSnapshot = targetSnapshot;
                this.mergeWorker = mergeWorker;
            }

            public override void addSource(MergeOperationContext operationContext, IIndexDefinition sourceDefinition)
            {
                if (sourceDefinition.Aliases != null)
                {
                    references.CollectMust(sourceDefinition.Aliases.MustAliases, targetSnapshot.CallLevel);
                    references.CollectMay(sourceDefinition.Aliases.MayAliases, targetSnapshot.CallLevel);

                    hasAliases = true;
                }
                else
                {
                    hasAliasesAlways = false;
                }
            }

            public override void provideCustomOperation(MemoryIndex targetIndex)
            {
                if (hasAliases)
                {
                    references.SetAliases(targetIndex, mergeWorker, hasAliasesAlways && !operation.IsUndefined);

                    MemoryAliasInfo aliasInfo;
                    if (mergeWorker.MemoryAliases.TryGetValue(targetIndex, out aliasInfo))
                    {
                        aliasInfo.IsTargetOfMerge = true;
                    }
                    else
                    {
                        throw new Exception("Alias merge - memory index was not included into collection of aliases");
                    }
                }
            }
        }
    }
}
