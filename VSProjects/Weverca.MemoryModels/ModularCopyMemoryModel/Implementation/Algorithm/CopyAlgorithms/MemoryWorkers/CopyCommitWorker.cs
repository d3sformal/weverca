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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// Implements logic of the copy implementation of the commit algorithm.
    /// </summary>
    class CopyCommitWorker
    {

        /// <summary>
        /// Gets or sets the factories.
        /// </summary>
        /// <value>
        /// The factories.
        /// </value>
        public ModularMemoryModelFactories Factories { get; set; }

        private ISnapshotStructureProxy newStructure, oldStructure;
        private ISnapshotDataProxy newData, oldData;

        private Snapshot snapshot;
        private int simplifyLimit;
        private MemoryAssistantBase assistant;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyCommitWorker"/> class.
        /// </summary>
        /// <param name="factories">The factories.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <param name="newStructure">The new structure.</param>
        /// <param name="oldStructure">The old structure.</param>
        /// <param name="newData">The new data.</param>
        /// <param name="oldData">The old data.</param>
        public CopyCommitWorker(ModularMemoryModelFactories factories, Snapshot snapshot, int simplifyLimit, 
            ISnapshotStructureProxy newStructure, ISnapshotStructureProxy oldStructure, 
            ISnapshotDataProxy newData, ISnapshotDataProxy oldData)
        {
            Factories = factories;
            this.snapshot = snapshot;
            this.simplifyLimit = simplifyLimit;
            this.assistant = snapshot.MemoryAssistant;

            this.newStructure = newStructure;
            this.oldStructure = oldStructure;
            this.newData = newData;
            this.oldData = oldData;
        }

        /// <summary>
        /// Compares the structure and simplifies the data.
        /// </summary>
        /// <param name="widen">if set to <c>true</c> then widening operation is performed.</param>
        /// <returns>true if memory state is different; otherwise false</returns>
        public bool CompareStructureAndSimplify(bool widen)
        {
            HashSet<MemoryIndex> usedIndexes = new HashSet<MemoryIndex>();
            CollectionMemoryUtils.AddAll(usedIndexes, newStructure.Readonly.Indexes);
            CollectionMemoryUtils.AddAll(usedIndexes, oldStructure.Readonly.Indexes);

            IIndexDefinition emptyDefinition = Factories.StructuralContainersFactories.IndexDefinitionFactory.CreateIndexDefinition(newStructure.Writeable);

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

            return !areEqual;
        }

        /// <summary>
        /// Compares the data and simplify.
        /// </summary>
        /// <param name="widen">if set to <c>true</c> then widening operation is performed.</param>
        /// <returns>true if memory state is different; otherwise false</returns>
        public bool CompareDataAndSimplify(bool widen)
        {
            bool areEquals = true;

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
                    areEquals = false;
                }
            }

            return !areEquals;
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

            if (ValueUtils.CompareMemoryEntries(newEntry, oldEntry))
            {
                return true;
            }
            else if (newEntry.Count > simplifyLimit)
            {
                MemoryEntry simplifiedEntry = assistant.Simplify(newEntry);
                MemoryEntry entry = setNewMemoryEntry(index, newEntry, simplifiedEntry);

                return ValueUtils.CompareMemoryEntries(entry, oldEntry);
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
                IObjectValueContainer objects = Factories.StructuralContainersFactories.ObjectValueContainerFactory.CreateObjectValueContainer(snapshot.Structure.Writeable, currentVisitor.Objects);
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
    }
}
