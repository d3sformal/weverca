/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms
{
    class TrackingCommitAlgorithm : ICommitAlgorithm, IAlgorithmFactory<ICommitAlgorithm>
    {
        private ISnapshotStructureProxy newStructure, oldStructure;
        private ISnapshotDataProxy newData, oldData;
        private bool areSame;
        private Snapshot snapshot;
        private IIndexDefinition emptyDefinition;

        /// <inheritdoc />
        public ICommitAlgorithm CreateInstance()
        {
            return new TrackingCommitAlgorithm();
        }

        /// <inheritdoc />
        public void SetStructure(ISnapshotStructureProxy newStructure, ISnapshotStructureProxy oldStructure)
        {
            emptyDefinition = newStructure.CreateIndexDefinition();

            this.newStructure = newStructure;
            this.oldStructure = oldStructure;
        }

        /// <inheritdoc />
        public void SetData(ISnapshotDataProxy newData, ISnapshotDataProxy oldData)
        {
            this.newData = newData;
            this.oldData = oldData;
        }

        /// <inheritdoc />
        public void CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            this.snapshot = snapshot;

            /*if (snapshot.NumberOfTransactions > 1)
            {*/
                switch (snapshot.CurrentMode)
                {
                    case SnapshotMode.MemoryLevel:
                        areSame = compareStructureAndSimplify(simplifyLimit, false, snapshot.MemoryAssistant);
                        break;

                    case SnapshotMode.InfoLevel:
                        areSame = compareDataAndSimplify(simplifyLimit, false, snapshot.MemoryAssistant);
                        break;

                    default:
                        throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
                }
            /*}
            else
            {
                areSame = false;
            }*/
        }

        /// <inheritdoc />
        public void CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            this.snapshot = snapshot;

            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    areSame = compareStructureAndSimplify(simplifyLimit, true, snapshot.MemoryAssistant);
                    break;

                case SnapshotMode.InfoLevel:
                    areSame = compareDataAndSimplify(simplifyLimit, true, snapshot.MemoryAssistant);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public bool IsDifferent()
        {
            return !areSame;
        }

        private bool compareStructureAndSimplify(int simplifyLimit, bool widen, MemoryAssistantBase assistant)
        {
            if (newStructure.IsReadonly && newData.IsReadonly)
            {
                if (snapshot.NumberOfTransactions > 1)
                {
                    bool differs = newStructure.Readonly.DiffersOnCommit || newData.Readonly.DiffersOnCommit;
                    return !differs;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                bool areSame = snapshot.NumberOfTransactions > 1;
                if (!newData.IsReadonly)
                {
                    if (widen)
                    {
                        widenAndSimplifyData(simplifyLimit, assistant);
                    }
                    else
                    {
                        simplifyData(simplifyLimit, assistant);
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
                return areSame;
            }
        }

        private bool compareDataAndSimplify(int simplifyLimit, bool widen, MemoryAssistantBase assistant)
        {
            if (newData.IsReadonly)
            {
                bool differs = newData.Readonly.DiffersOnCommit;
                return !differs;
            }
            else
            {
                bool areSame = snapshot.NumberOfTransactions > 1;
                if (!newData.IsReadonly)
                {
                    if (widen)
                    {
                        widenAndSimplifyData(simplifyLimit, assistant);
                    }
                    else
                    {
                        simplifyData(simplifyLimit, assistant);
                    }

                    if (areSame)
                    {
                        areSame = compareData();
                    }

                    newData.Writeable.SetDiffersOnCommit(!areSame);
                }
                return areSame;
            }
        }

        private void widenAndSimplifyData(int simplifyLimit, MemoryAssistantBase assistant)
        {
            List<MemoryIndex> indexes = new List<MemoryIndex>();
            CollectionTools.AddAll(indexes, newData.Readonly.ReadonlyChangeTracker.IndexChanges);

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

                if (accEntryValue != newEntry)
                {
                    setNewMemoryEntry(index, newEntry, accEntryValue);
                }
            }
        }

        private void simplifyData(int simplifyLimit, MemoryAssistantBase assistant)
        {
            List<MemoryIndex> indexes = new List<MemoryIndex>();
            CollectionTools.AddAll(indexes, newData.Readonly.ReadonlyChangeTracker.IndexChanges);
            
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
                CollectionTools.AddAll(indexes, newStructure.Readonly.ReadonlyChangeTracker.IndexChanges);

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
                CollectionTools.AddAll(indexes, newStructure.Readonly.ReadonlyChangeTracker.IndexChanges);

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
                IObjectValueContainer objects = snapshot.Structure.CreateObjectValueContainer(currentVisitor.Objects);
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

                CollectionTools.AddAll(indexChanges, trackerA.IndexChanges);
                CollectionTools.AddAllIfNotNull(functionChanges, trackerA.FunctionChanges);
                CollectionTools.AddAllIfNotNull(classChanges, trackerA.ClassChanges);
                
                trackerA = trackerA.PreviousTracker;
            }
        }
    }
}