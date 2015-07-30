using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.GraphVisualizer;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers.Merge
{
    class TrackingCallMergeStructureWorker : AbstractTrackingMergeWorker, IReferenceHolder
    {
        private Snapshot callSnapshot;

        internal Dictionary<MemoryIndex, MemoryAliasInfo> MemoryAliases { get; private set; }


        public TrackingCallMergeStructureWorker(ModularMemoryModelFactories factories, Snapshot targetSnapshot, Snapshot callSnapshot, List<Snapshot> sourceSnapshots)
            : base(factories, targetSnapshot, sourceSnapshots, true)
        {
            this.callSnapshot = callSnapshot;

            MemoryAliases = new Dictionary<MemoryIndex, MemoryAliasInfo>();
            Factories = factories;
        }

        public void MergeStructure()
        {
            isStructureWriteable = true;

            createSnapshotContexts();
            collectStructureChanges();

            createNewStructure();

            mergeDeclarations();
            mergeObjectDefinitions();
            mergeMemoryStacksRoots();

            processMergeOperations();

            updateAliases();
            storeLocalArays();

            ensureTrackingChanges();
        }

        private void collectStructureChanges()
        {
            List<MemoryIndexTree> changes = new List<MemoryIndexTree>();
            var ancestor = callSnapshot.Structure.Readonly.ReadonlyChangeTracker;
            for (int x = 0; x < snapshotContexts.Count; x++)
            {
                SnapshotContext context = snapshotContexts[x];

                MemoryIndexTree currentChanges = context.ChangedIndexesTree;
                changes.Add(currentChanges);

                collectSingleFunctionChanges(callSnapshot, context.SourceStructure.ReadonlyChangeTracker, currentChanges, changes);
                //ancestor = getFirstCommonAncestor(context.SourceStructure.ReadonlyChangeTracker, ancestor, currentChanges, changes);
            }

            if (targetSnapshot.StructureCallChanges != null)
            {
                CollectionMemoryUtils.AddAll(this.changeTree, targetSnapshot.StructureCallChanges);
            }

            targetSnapshot.StructureCallChanges = changeTree.StoredIndexes;
        }

        private void createNewStructure()
        {
            Structure = Factories.SnapshotStructureFactory.CopyInstance(callSnapshot.Structure);

            writeableTargetStructure = Structure.Writeable;
            targetStructure = writeableTargetStructure;
        }

        private void updateAliases()
        {
            foreach (var item in MemoryAliases)
            {
                MemoryIndex targetIndex = item.Key;
                MemoryAliasInfo aliasInfo = item.Value;

                if (!aliasInfo.IsTargetOfMerge)
                {
                    IMemoryAlias currentAliases;
                    if (writeableTargetStructure.TryGetAliases(targetIndex, out currentAliases))
                    {
                        aliasInfo.Aliases.MayAliases.AddAll(currentAliases.MayAliases);
                        aliasInfo.Aliases.MustAliases.AddAll(currentAliases.MustAliases);
                    }
                }
                foreach (MemoryIndex alias in aliasInfo.RemovedAliases)
                {
                    aliasInfo.Aliases.MustAliases.Remove(alias);
                    aliasInfo.Aliases.MayAliases.Remove(alias);
                }

                writeableTargetStructure.SetAlias(targetIndex, aliasInfo.Aliases.Build(writeableTargetStructure));
            }
        }

        private void mergeDeclarations()
        {
            foreach (QualifiedName functionName in functionChages)
            {
                HashSet<FunctionValue> declarations = new HashSet<FunctionValue>();
                foreach (var context in snapshotContexts)
                {
                    IEnumerable<FunctionValue> decl;
                    if (context.SourceStructure.TryGetFunction(functionName, out decl))
                    {
                        CollectionMemoryUtils.AddAll(declarations, decl);
                    }
                }
                writeableTargetStructure.SetFunctionDeclarations(functionName, declarations);
            }

            foreach (QualifiedName className in classChanges)
            {
                HashSet<TypeValue> declarations = new HashSet<TypeValue>();
                foreach (var context in snapshotContexts)
                {
                    IEnumerable<TypeValue> decl;
                    if (context.SourceStructure.TryGetClass(className, out decl))
                    {
                        CollectionMemoryUtils.AddAll(declarations, decl);
                    }
                }
                writeableTargetStructure.SetClassDeclarations(className, declarations);
            }
        }

        private void ensureTrackingChanges()
        {
            foreach (MemoryIndex index in this.changeTree.StoredIndexes)
            {
                writeableTargetStructure.WriteableChangeTracker.ModifiedIndex(index);
            }

            foreach (QualifiedName name in functionChages)
            {
                writeableTargetStructure.WriteableChangeTracker.ModifiedFunction(name);
            }

            foreach (QualifiedName name in classChanges)
            {
                writeableTargetStructure.WriteableChangeTracker.ModifiedClass(name);
            }
        }

        private void storeLocalArays()
        {
            foreach (var context in snapshotContexts)
            {
                foreach (AssociativeArray array in context.SourceStructure.ReadonlyLocalContext.ReadonlyArrays)
                {
                    Structure.Writeable.AddCallArray(array, context.SourceSnapshot);
                }
            }
        }

        protected override TrackingMergeWorkerOperationAccessor createNewOperationAccessor(MergeOperation operation)
        {
            return new OperationAccessor(operation, targetSnapshot, writeableTargetStructure, this);
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
            MemoryAliasInfo aliasInfo = getAliasInfo(index);
            IMemoryAliasBuilder alias = aliasInfo.Aliases;

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
            MemoryAliasInfo aliasInfo = getAliasInfo(index);
            IMemoryAliasBuilder alias = aliasInfo.Aliases;

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

        internal MemoryAliasInfo getAliasInfo(MemoryIndex index)
        {
            MemoryAliasInfo aliasInfo;
            if (!MemoryAliases.TryGetValue(index, out aliasInfo))
            {
                IMemoryAliasBuilder alias = Factories.StructuralContainersFactories.MemoryAliasFactory.CreateMemoryAlias(writeableTargetStructure, index).Builder(writeableTargetStructure);
                aliasInfo = new MemoryAliasInfo(alias, false);

                MemoryAliases[index] = aliasInfo;
            }
            return aliasInfo;
        }

        private class OperationAccessor : TrackingMergeWorkerOperationAccessor
        {

            private ReferenceCollector references;

            private MergeOperation operation;

            private Snapshot targetSnapshot;
            private TrackingCallMergeStructureWorker mergeWorker;

            private bool hasAliasesAlways = true;
            private IWriteableSnapshotStructure writeableTargetStructure;

            public OperationAccessor(MergeOperation operation, Snapshot targetSnapshot, IWriteableSnapshotStructure writeableTargetStructure, TrackingCallMergeStructureWorker mergeWorker)
            {
                this.operation = operation;
                this.targetSnapshot = targetSnapshot;
                this.writeableTargetStructure = writeableTargetStructure;
                this.mergeWorker = mergeWorker;

                this.references = new ReferenceCollector(writeableTargetStructure);
            }

            public override void addSource(MergeOperationContext operationContext, IIndexDefinition sourceDefinition)
            {
                if (sourceDefinition.Aliases != null)
                {
                    references.CollectMust(sourceDefinition.Aliases.MustAliases);
                    references.CollectMay(sourceDefinition.Aliases.MayAliases);
                }
                else
                {
                    hasAliasesAlways = false;
                }
            }

            public override void provideCustomOperation(MemoryIndex targetIndex)
            {
                if (references.HasAliases)
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

            public override void provideCustomDeleteOperation(MemoryIndex targetIndex, IIndexDefinition targetDefinition)
            {
                if (targetDefinition.Array != null)
                {
                    writeableTargetStructure.RemoveArray(targetIndex, targetDefinition.Array);
                }

                if (targetDefinition.Aliases != null)
                {
                    foreach (MemoryIndex aliasIndex in targetDefinition.Aliases.MustAliases)
                    {
                        MemoryAliasInfo aliasInfo = mergeWorker.getAliasInfo(aliasIndex);
                        aliasInfo.AddRemovedAlias(targetIndex);
                    }

                    foreach (MemoryIndex aliasIndex in targetDefinition.Aliases.MayAliases)
                    {
                        MemoryAliasInfo aliasInfo = mergeWorker.getAliasInfo(aliasIndex);
                        aliasInfo.AddRemovedAlias(targetIndex);
                    }
                }

                writeableTargetStructure.RemoveIndex(targetIndex);
            }
        }

        protected override MemoryIndex createNewTargetIndex(ITargetContainerContext targetContainerContext, string childName)
        {
            MemoryIndex targetIndex = targetContainerContext.createMemoryIndex(childName);
            targetContainerContext.getWriteableSourceContainer().AddIndex(childName, targetIndex);

            return targetIndex;
        }

        protected override void deleteChild(ITargetContainerContext targetContainerContext, string childName)
        {
            targetContainerContext.getWriteableSourceContainer().RemoveIndex(childName);
        }

        protected override bool MissingStacklevel(int stackLevel)
        {
            /*if (ensureAllStackContexts)
            {
                writeableTargetStructure.AddStackLevel(stackLevel);
                return true;
            }
            else
            {
                return false;
            }*/

            return false;
        }
    }
}
