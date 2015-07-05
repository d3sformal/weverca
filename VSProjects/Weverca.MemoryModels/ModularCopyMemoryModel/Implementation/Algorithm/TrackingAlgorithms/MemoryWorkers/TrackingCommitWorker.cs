using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers
{
    class TrackingCommitWorker
    {
        private ISnapshotStructureProxy newStructure, oldStructure;
        private ISnapshotDataProxy newData, oldData;

        private Snapshot snapshot;
        private IIndexDefinition emptyDefinition;
        private int simplifyLimit;
        private MemoryAssistantBase assistant;

        public ModularMemoryModelFactories Factories { get; set; }

        public TrackingCommitWorker(ModularMemoryModelFactories factories, Snapshot snapshot, int simplifyLimit, 
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

            Factories = factories;
        }

        public bool CompareStructureAndSimplify(bool widen)
        {
            if (newStructure.IsReadonly && newData.IsReadonly)
            {
                if (snapshot.NumberOfTransactions > 1)
                {
                    bool differs = newStructure.Readonly.DiffersOnCommit || newData.Readonly.DiffersOnCommit;
                    return differs;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                bool areSame = snapshot.NumberOfTransactions > 1;
                if (!newData.IsReadonly)
                {
                    if (widen)
                    {
                        widenAndSimplifyData();
                    }
                    else
                    {
                        simplifyData();
                    }

                    if (areSame)
                    {
                        areSame = compareData();
                    }

                    newData.Writeable.SetDiffersOnCommit(!areSame);
                }

                if (!newStructure.IsReadonly)
                {
                    clearStructureTracker();

                    if (areSame)
                    {
                        areSame = compareStructure();
                    }

                    newStructure.Writeable.SetDiffersOnCommit(!areSame);
                }
                return !areSame;
            }
        }

        public bool CompareDataAndSimplify(bool widen)
        {
            if (newData.IsReadonly)
            {
                bool differs = newData.Readonly.DiffersOnCommit;
                return differs;
            }
            else
            {
                bool areSame = snapshot.NumberOfTransactions > 1;
                if (!newData.IsReadonly)
                {
                    if (widen)
                    {
                        widenAndSimplifyData();
                    }
                    else
                    {
                        simplifyData();
                    }

                    if (areSame)
                    {
                        areSame = compareData();
                    }

                    newData.Writeable.SetDiffersOnCommit(!areSame);
                }
                return !areSame;
            }
        }



        private void widenAndSimplifyData()
        {
            List<MemoryIndex> indexes = new List<MemoryIndex>();
            CollectionMemoryUtils.AddAll(indexes, newData.Readonly.ReadonlyChangeTracker.IndexChanges);

            var previousTracker = newData.Readonly.ReadonlyChangeTracker.PreviousTracker;
            var currentTracker = newData.Writeable.WriteableChangeTracker;

            foreach (MemoryIndex index in indexes)
            {
                MemoryEntry newEntry = getMemoryEntryOrEmpty(index, newData.Readonly);
                MemoryEntry accEntryValue = newEntry;

                if (newEntry != null && newEntry.Count > simplifyLimit)
                {
                    MemoryEntry simplifiedEntry = assistant.Simplify(newEntry);
                    accEntryValue = simplifiedEntry;
                }

                MemoryEntry oldEntry = getMemoryEntryOrEmpty(index, oldData.Readonly);
                if (!compareMemoryEntries(newEntry, oldEntry))
                {
                    MemoryEntry widenedEntry = assistant.Widen(oldEntry, newEntry);
                    accEntryValue = widenedEntry;
                }

                if (previousTracker != null)
                {
                    MemoryEntry previousEntry = getMemoryEntryOrEmpty(index, previousTracker.Container);
                    if (compareMemoryEntries(previousEntry, accEntryValue))
                    {
                        currentTracker.RemoveIndexChange(index);
                    }
                }

                if (accEntryValue != newEntry && newEntry != null)
                {
                    setNewMemoryEntry(index, newEntry, accEntryValue);
                }
            }
        }

        private void simplifyData()
        {
            List<MemoryIndex> indexes = new List<MemoryIndex>();
            CollectionMemoryUtils.AddAll(indexes, newData.Readonly.ReadonlyChangeTracker.IndexChanges);

            var previousTracker = newData.Readonly.ReadonlyChangeTracker.PreviousTracker;
            var currentTracker = newData.Writeable.WriteableChangeTracker;

            foreach (MemoryIndex index in indexes)
            {
                MemoryEntry newEntry = getMemoryEntryOrEmpty(index, newData.Readonly);
                if (newEntry != null && newEntry.Count > simplifyLimit)
                {
                    MemoryEntry simplifiedEntry = assistant.Simplify(newEntry);
                    setNewMemoryEntry(index, newEntry, simplifiedEntry);

                    newEntry = simplifiedEntry;
                }

                if (previousTracker != null)
                {
                    MemoryEntry previousEntry = getMemoryEntryOrEmpty(index, previousTracker.Container);
                    if (compareMemoryEntries(previousEntry, newEntry))
                    {
                        currentTracker.RemoveIndexChange(index);
                    }
                }
            }
        }

        private void clearStructureTracker()
        {
            var previousTracker = newStructure.Readonly.ReadonlyChangeTracker.PreviousTracker;
            if (previousTracker != null)
            {
                List<MemoryIndex> indexes = new List<MemoryIndex>();
                CollectionMemoryUtils.AddAll(indexes, newStructure.Readonly.ReadonlyChangeTracker.IndexChanges);

                IReadOnlySnapshotStructure previousStructure = previousTracker.Container;
                foreach (MemoryIndex index in indexes)
                {
                    IIndexDefinition newDefinition = getIndexDefinitionOrUndefined(index, newStructure.Readonly);
                    IIndexDefinition previousDefinition = getIndexDefinitionOrUndefined(index, previousStructure);

                    if (compareIndexDefinitions(newDefinition, previousDefinition))
                    {
                        newStructure.Writeable.WriteableChangeTracker.RemoveIndexChange(index);
                    }
                }
            }
        }

        private void clearDataTracker()
        {
            var previousTracker = newData.Readonly.ReadonlyChangeTracker.PreviousTracker;
            if (previousTracker != null)
            {
                List<MemoryIndex> indexes = new List<MemoryIndex>();
                CollectionMemoryUtils.AddAll(indexes, newStructure.Readonly.ReadonlyChangeTracker.IndexChanges);

                IReadOnlySnapshotData previousData = previousTracker.Container;
                foreach (MemoryIndex index in indexes)
                {
                    if (index is TemporaryIndex)
                    {
                        newStructure.Writeable.WriteableChangeTracker.RemoveIndexChange(index);
                    }
                    else
                    {
                        MemoryEntry newEntry = getMemoryEntryOrEmpty(index, newData.Readonly);
                        MemoryEntry previousEntry = getMemoryEntryOrEmpty(index, previousData);

                        if (compareMemoryEntries(newEntry, previousEntry))
                        {
                            newData.Writeable.WriteableChangeTracker.RemoveIndexChange(index);
                        }
                    }
                }
            }
        }





        private bool compareData()
        {
            HashSet<MemoryIndex> indexChanges = new HashSet<MemoryIndex>();
            /*HashSet<QualifiedName> functionChanges = new HashSet<QualifiedName>();
            HashSet<QualifiedName> classChanges = new HashSet<QualifiedName>();

            collectChanges(
                newData.Readonly.ReadonlyChangeTracker,
                oldData.Readonly.ReadonlyChangeTracker,
                indexChanges, functionChanges, classChanges);*/

            var trackerA = newData.Readonly.ReadonlyChangeTracker;
            var trackerB = oldData.Readonly.ReadonlyChangeTracker;
            while (trackerA != trackerB)
            {
                if (trackerA == null || trackerB != null && trackerA.TrackerId < trackerB.TrackerId)
                {
                    var swap = trackerA;
                    trackerA = trackerB;
                    trackerB = swap;
                }

                foreach (MemoryIndex index in trackerA.IndexChanges)
                {
                    if (!indexChanges.Contains(index))
                    {
                        MemoryEntry newEntry = getMemoryEntryOrEmpty(index, newData.Readonly);
                        MemoryEntry oldEntry = getMemoryEntryOrEmpty(index, oldData.Readonly);

                        if (!compareMemoryEntries(newEntry, oldEntry))
                        {
                            return false;
                        }

                        indexChanges.Add(index);
                    }
                }

                trackerA = trackerA.PreviousTracker;
            }


            /*foreach (MemoryIndex index in indexChanges)
            {
                if (index is TemporaryIndex)
                {
                    continue;
                }

                MemoryEntry newEntry = getMemoryEntryOrEmpty(index, newData.Readonly);
                MemoryEntry oldEntry = getMemoryEntryOrEmpty(index, oldData.Readonly);

                if (!compareMemoryEntries(newEntry, oldEntry))
                {
                    return false;
                }
            }*/

            return true;
        }

        private bool compareStructure()
        {
            HashSet<MemoryIndex> indexChanges = new HashSet<MemoryIndex>();
            var trackerA = newStructure.Readonly.ReadonlyChangeTracker;
            var trackerB = oldStructure.Readonly.ReadonlyChangeTracker;
            while (trackerA != trackerB)
            {
                if (trackerA == null || trackerB != null && trackerA.TrackerId < trackerB.TrackerId)
                {
                    var swap = trackerA;
                    trackerA = trackerB;
                    trackerB = swap;
                }

                foreach (MemoryIndex index in trackerA.IndexChanges)
                {
                    if (!indexChanges.Contains(index))
                    {
                        IIndexDefinition newDefinition = getIndexDefinitionOrUndefined(index, newStructure.Readonly);
                        IIndexDefinition oldDefinition = getIndexDefinitionOrUndefined(index, oldStructure.Readonly);

                        if (!compareIndexDefinitions(newDefinition, oldDefinition))
                        {
                            return false;
                        }

                        indexChanges.Add(index);
                    }
                }

                trackerA = trackerA.PreviousTracker;
            }
            return true;
        }

        private MemoryEntry setNewMemoryEntry(MemoryIndex index, MemoryEntry currentEntry, MemoryEntry modifiedEntry)
        {
            CollectComposedValuesVisitor currentVisitor = new CollectComposedValuesVisitor();
            currentVisitor.VisitMemoryEntry(currentEntry);

            CollectComposedValuesVisitor modifiedVisitor = new CollectComposedValuesVisitor();
            modifiedVisitor.VisitMemoryEntry(modifiedEntry);

            if (currentVisitor.Arrays.Count != modifiedVisitor.Arrays.Count)
            {
                snapshot.DestroyArray(index);
            }

            if (modifiedVisitor.Objects.Count != currentVisitor.Objects.Count)
            {
                IObjectValueContainer objects = Factories.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(newStructure.Writeable, currentVisitor.Objects);
                snapshot.Structure.Writeable.SetObjects(index, objects);
            }

            newData.Writeable.SetMemoryEntry(index, modifiedEntry);
            return modifiedEntry;
        }

        private IIndexDefinition getIndexDefinitionOrUndefined(MemoryIndex index, IReadOnlySnapshotStructure snapshotStructure)
        {
            IIndexDefinition definition = null;
            if (!snapshotStructure.TryGetIndexDefinition(index, out definition))
            {
                definition = null;
            }

            return definition;
        }

        private MemoryEntry getMemoryEntryOrEmpty(MemoryIndex index, IReadOnlySnapshotData snapshotdata)
        {
            MemoryEntry entry = null;
            if (!snapshotdata.TryGetMemoryEntry(index, out entry))
            {
                entry = null;
            }

            return entry;
        }

        private bool compareMemoryEntries(MemoryEntry newEntry, MemoryEntry oldEntry)
        {
            if (newEntry == oldEntry)
            {
                return true;
            }

            if (newEntry == null || oldEntry == null)
            {
                return false;
            }

            if (newEntry.Count != oldEntry.Count)
            {
                return false;
            }

            if (newEntry.ContainsAssociativeArray != oldEntry.ContainsAssociativeArray)
            {
                return false;
            }

            HashSet<Value> oldValues = new HashSet<Value>(oldEntry.PossibleValues);
            foreach (Value value in newEntry.PossibleValues)
            {
                if (!(value is AssociativeArray))
                {
                    if (!oldValues.Contains(value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool compareIndexDefinitions(IIndexDefinition newDefinition, IIndexDefinition oldDefinition)
        {
            if (newDefinition == oldDefinition)
            {
                return true;
            }

            if (newDefinition == null || oldDefinition == null)
            {
                return false;
            }

            if (newDefinition.Array != oldDefinition.Array)
            {
                if (newDefinition.Array == null || oldDefinition.Array == null)
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

        protected void collectChanges<T>(
            IReadonlyChangeTracker<T> trackerA,
            IReadonlyChangeTracker<T> trackerB,
            ICollection<MemoryIndex> indexChanges,
            ICollection<QualifiedName> functionChanges,
            ICollection<QualifiedName> classChanges) where T : class
        {
            while (trackerA != trackerB)
            {
                if (trackerA == null || trackerB != null && trackerA.TrackerId < trackerB.TrackerId)
                {
                    var swap = trackerA;
                    trackerA = trackerB;
                    trackerB = swap;
                }

                CollectionMemoryUtils.AddAll(indexChanges, trackerA.IndexChanges);
                CollectionMemoryUtils.AddAllIfNotNull(functionChanges, trackerA.FunctionChanges);
                CollectionMemoryUtils.AddAllIfNotNull(classChanges, trackerA.ClassChanges);

                trackerA = trackerA.PreviousTracker;
            }
        }
    }
}
