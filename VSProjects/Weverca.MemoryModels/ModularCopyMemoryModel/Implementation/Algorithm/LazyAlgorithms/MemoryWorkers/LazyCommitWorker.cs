using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers
{
    class LazyCommitWorker
    {
        private ISnapshotStructureProxy newStructure, oldStructure;
        private ISnapshotDataProxy newData, oldData;

        private Snapshot snapshot;
        private IIndexDefinition emptyDefinition;
        private int simplifyLimit;
        private MemoryAssistantBase assistant;

        public LazyCommitWorker(Snapshot snapshot, int simplifyLimit, 
            ISnapshotStructureProxy newStructure, ISnapshotStructureProxy oldStructure, 
            ISnapshotDataProxy newData, ISnapshotDataProxy oldData)
        {
            this.snapshot = snapshot;
            this.simplifyLimit = simplifyLimit;
            this.assistant = snapshot.MemoryAssistant;

            this.newStructure = newStructure;
            this.oldStructure = oldStructure;
            this.newData = newData;
            this.oldData = oldData;
        }

        public bool CompareStructureAndSimplify(bool widen)
        {
            if (snapshot.NumberOfTransactions == 1)
            {
                return true;
            }

            if (newStructure.IsReadonly && newData.IsReadonly)
            {
                bool differs = newStructure.Readonly.DiffersOnCommit || newData.Readonly.DiffersOnCommit;
                return differs;
            }

            HashSet<MemoryIndex> usedIndexes = new HashSet<MemoryIndex>();
            CollectionMemoryUtils.AddAll(usedIndexes, newStructure.Readonly.Indexes);
            CollectionMemoryUtils.AddAll(usedIndexes, oldStructure.Readonly.Indexes);

            IIndexDefinition emptyDefinition = snapshot.MemoryModelFactory.StructuralContainersFactories.IndexDefinitionFactory.CreateIndexDefinition(newStructure.Writeable);

            bool areEqual = true;

            foreach (MemoryIndex index in usedIndexes)
            {
                if (index is TemporaryIndex)
                {
                    continue;
                }

                IIndexDefinition newDefinition = getIndexDefinitionOrUndefined(index, newStructure, emptyDefinition);
                IIndexDefinition oldDefinition = getIndexDefinitionOrUndefined(index, oldStructure, emptyDefinition);

                if (widen)
                {
                    if (!compareData(index))
                    {
                        widenData(index);
                    }
                }

                if (!compareIndexDefinitions(newDefinition, oldDefinition))
                {
                    areEqual = false;
                }

                if (!compareData(index))
                {
                    areEqual = false;
                }
            }

            if (!newStructure.IsReadonly)
            {
                newStructure.Writeable.SetDiffersOnCommit(!areEqual);
            }
            if (!newData.IsReadonly)
            {
                newData.Writeable.SetDiffersOnCommit(!areEqual);
            }
            return !areEqual;
        }

        public bool CompareDataAndSimplify(bool widen)
        {
            if (snapshot.NumberOfTransactions == 1)
            {
                return true;
            }

            if (newData.IsReadonly)
            {
                bool differs = newData.Readonly.DiffersOnCommit;
                return differs;
            }

            bool areEqual = true;

            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            CollectionMemoryUtils.AddAll(indexes, newData.Readonly.Indexes);
            CollectionMemoryUtils.AddAll(indexes, oldData.Readonly.Indexes);

            foreach (MemoryIndex index in indexes)
            {
                if (widen)
                {
                    if (!compareData(index))
                    {
                        widenData(index);
                    }
                }

                if (!compareData(index))
                {
                    areEqual = false;
                }
            }

            if (!newData.IsReadonly)
            {
                newData.Writeable.SetDiffersOnCommit(!areEqual);
            }
            return !areEqual;
        }

        private void widenData(MemoryIndex index)
        {
            MemoryEntry newEntry = null;
            if (!newData.Readonly.TryGetMemoryEntry(index, out newEntry))
            {
                newEntry = snapshot.EmptyEntry;
            }

            MemoryEntry oldEntry = null;
            if (!oldData.Readonly.TryGetMemoryEntry(index, out oldEntry))
            {
                oldEntry = snapshot.EmptyEntry;
            }

            MemoryEntry widenedEntry = assistant.Widen(oldEntry, newEntry);
            MemoryEntry entry = setNewMemoryEntry(index, newEntry, widenedEntry);
        }

        private bool compareData(MemoryIndex index)
        {
            MemoryEntry newEntry = null;
            if (!newData.Readonly.TryGetMemoryEntry(index, out newEntry))
            {
                newEntry = snapshot.EmptyEntry;
            }

            MemoryEntry oldEntry = null;
            if (!oldData.Readonly.TryGetMemoryEntry(index, out oldEntry))
            {
                oldEntry = snapshot.EmptyEntry;
            }

            if (oldEntry.Equals(newEntry))
            {
                return true;
            }
            else if (newEntry.Count > simplifyLimit)
            {
                MemoryEntry simplifiedEntry = assistant.Simplify(newEntry);
                MemoryEntry entry = setNewMemoryEntry(index, newEntry, simplifiedEntry);

                return oldEntry.Equals(entry);
            }
            else
            {
                return false;
            }
        }

        private MemoryEntry setNewMemoryEntry(MemoryIndex index, MemoryEntry currentEntry, MemoryEntry modifiedEntry)
        {
            CollectComposedValuesVisitor currentVisitor = new CollectComposedValuesVisitor();
            currentVisitor.VisitMemoryEntry(currentEntry);

            CollectComposedValuesVisitor mdifiedVisitor = new CollectComposedValuesVisitor();
            mdifiedVisitor.VisitMemoryEntry(modifiedEntry);

            if (currentVisitor.Arrays.Count != mdifiedVisitor.Arrays.Count)
            {
                snapshot.DestroyArray(index);
            }

            if (mdifiedVisitor.Objects.Count != currentVisitor.Objects.Count)
            {
                IObjectValueContainer objects = snapshot.MemoryModelFactory.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(snapshot.Structure.Writeable, currentVisitor.Objects);
                snapshot.Structure.Writeable.SetObjects(index, objects);
            }

            newData.Writeable.SetMemoryEntry(index, modifiedEntry);
            return modifiedEntry;
        }

        private IIndexDefinition getIndexDefinitionOrUndefined(MemoryIndex index, ISnapshotStructureProxy snapshotStructure, IIndexDefinition emptyDefinition)
        {
            IIndexDefinition definition = null;
            if (!snapshotStructure.Readonly.TryGetIndexDefinition(index, out definition))
            {
                definition = emptyDefinition;
            }

            return definition;
        }

        private bool compareIndexDefinitions(IIndexDefinition newDefinition, IIndexDefinition oldDefinition)
        {
            if (newDefinition == oldDefinition)
            {
                return true;
            }

            if (newDefinition.Array != oldDefinition.Array)
            {
                if (newDefinition.Array != null && oldDefinition.Array != null)
                {
                    if (!newDefinition.Array.Equals(oldDefinition.Array))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (newDefinition.Aliases != oldDefinition.Aliases)
            {
                if (newDefinition.Aliases != null && oldDefinition.Aliases != null)
                {
                    if (!compareSets(newDefinition.Aliases.MayAliases, oldDefinition.Aliases.MayAliases)
                        || !compareSets(newDefinition.Aliases.MustAliases, oldDefinition.Aliases.MustAliases)
                        )
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (newDefinition.Objects != oldDefinition.Objects)
            {
                if (!compareSets(newDefinition.Objects, oldDefinition.Objects))
                {
                    return false;
                }
            }

            return true;
        }

        private bool compareSets<T>(IReadonlySet<T> setA, IReadonlySet<T> setB)
        {
            if (setA == setB)
            {
                return true;
            }

            if (setA == null || setB == null)
            {
                return false;
            }

            if (setA.Count != setB.Count)
            {
                return false;
            }

            foreach (T value in setA)
            {
                if (!setB.Contains(value))
                {
                    return false;
                }
            }

            foreach (T value in setB)
            {
                if (!setA.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
