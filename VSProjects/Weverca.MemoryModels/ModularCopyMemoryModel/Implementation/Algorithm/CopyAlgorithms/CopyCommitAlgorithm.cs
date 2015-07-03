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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyCommitAlgorithm : ICommitAlgorithm, IAlgorithmFactory<ICommitAlgorithm>
    {
        private ISnapshotStructureProxy newStructure, oldStructure;
        private ISnapshotDataProxy newData, oldData;
        private bool areSame;
        private Snapshot snapshot;

        /// <inheritdoc />
        public ICommitAlgorithm CreateInstance()
        {
            return new CopyCommitAlgorithm();
        }

        /// <inheritdoc />
        public void SetStructure(ISnapshotStructureProxy newStructure, ISnapshotStructureProxy oldStructure)
        {
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
            /*if (newStructure.IsReadonly && newData.IsReadonly)
            {
                return true;
            }*/

            HashSet<MemoryIndex> usedIndexes = new HashSet<MemoryIndex>();
            CollectionMemoryUtils.AddAll(usedIndexes, newStructure.Readonly.Indexes);
            CollectionMemoryUtils.AddAll(usedIndexes, oldStructure.Readonly.Indexes);

            IIndexDefinition emptyDefinition = newStructure.CreateIndexDefinition();

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
                    if (!compareData(index, simplifyLimit, assistant))
                    {
                        widenData(index, simplifyLimit, assistant);
                    }
                }

                if (!compareIndexDefinitions(newDefinition, oldDefinition))
                {
                    areEqual = false;
                }

                if (!compareData(index, simplifyLimit, assistant))
                {
                    areEqual = false;
                }
            }

            return areEqual;
        }

        private bool compareDataAndSimplify(int simplifyLimit, bool widen, MemoryAssistantBase assistant)
        {
            /*if (newData.IsReadonly)
            {
                return true;
            }*/

            bool areEquals = true;

            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            CollectionMemoryUtils.AddAll(indexes, newData.Readonly.Indexes);
            CollectionMemoryUtils.AddAll(indexes, oldData.Readonly.Indexes);

            foreach (MemoryIndex index in indexes)
            {
                if (widen)
                {
                    if (!compareData(index, simplifyLimit, assistant))
                    {
                        widenData(index, simplifyLimit, assistant);
                    }
                }

                if (!compareData(index, simplifyLimit, assistant))
                {
                    areEquals = false;
                }
            }

            return areEquals;
        }

        private void widenData(MemoryIndex index, int simplifyLimit, MemoryAssistantBase assistant)
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

        private bool compareData(MemoryIndex index, int simplifyLimit, MemoryAssistantBase assistant)
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
                IObjectValueContainer objects = snapshot.Structure.CreateObjectValueContainer(currentVisitor.Objects);
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