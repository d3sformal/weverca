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
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Contains data for all definned memory location in memory snapshot.
    /// 
    /// Implemented as associative array which maps memory indexes to memory entries with data.
    /// </summary>
    public class SnapshotData
    {
        /// <summary>
        /// The empty entry with undefined value
        /// </summary>
        public readonly MemoryEntry EmptyEntry;

        /// <summary>
        /// Incremental counter for data unique identifier.
        /// </summary>
        static int DATA_ID = 0;

        /// <summary>
        /// The unique identifier for the each data instance
        /// </summary>
        int dataId = DATA_ID++;

        /// <summary>
        /// Associative container with memory entries for all memory locations.
        /// </summary>
        internal Dictionary<MemoryIndex, MemoryEntry> IndexData;

        /// <summary>
        /// Snapshot the data belongs to
        /// </summary>
        Snapshot snapshot;

        /// <summary>
        /// Prevents a default instance of the <see cref="SnapshotData"/> class from being created.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        private SnapshotData(Snapshot snapshot)
        {
            this.snapshot = snapshot;
            EmptyEntry = new MemoryEntry();
        }

        /// <summary>
        /// Creates the new data instance empty collection with no data.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New data instance empty collection with no data.</returns>
        public static SnapshotData CreateEmpty(Snapshot snapshot)
        {
            SnapshotData data = new SnapshotData(snapshot);

            data.IndexData = new Dictionary<MemoryIndex, MemoryEntry>();

            return data;
        }

        /// <summary>
        /// Creates new data instance and copies data from this collection to the new one.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New data instance and copies data from this collection to the new one.</returns>
        public SnapshotData Copy(Snapshot snapshot)
        {
            SnapshotData data = new SnapshotData(snapshot);

            data.IndexData = new Dictionary<MemoryIndex, MemoryEntry>(IndexData);

            return data;
        }

        #region MemoryEntries

        /// <summary>
        /// Gets the memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Memory entry for the given index.</returns>
        internal MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            MemoryEntry memoryEntry;
            if (TryGetMemoryEntry(index, out memoryEntry))
            {
                return memoryEntry;
            }
            else
            {
                return EmptyEntry;
            }
        }

        /// <summary>
        /// Tries to get memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>True whether collection contains memory entry for the given memory index.</returns>
        internal bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            return IndexData.TryGetValue(index, out entry);
        }

        /// <summary>
        /// Sets the memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="memoryEntry">The memory entry.</param>
        internal void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            IndexData[index] = memoryEntry;
        }

        /// <summary>
        /// Removes the memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        internal void RemoveMemoryEntry(MemoryIndex index)
        {
            IndexData.Remove(index);
        }

        #endregion

        /// <summary>
        /// Compares data of this data collection object with the given one.
        /// </summary>
        /// <param name="oldData">The old data.</param>
        /// <returns>True whether compared data are the same.</returns>
        public bool DataEquals(SnapshotData oldData)
        {
            if (this.IndexData.Count != oldData.IndexData.Count)
            {
                return false;
            }

            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(indexes, this.IndexData.Keys);
            HashSetTools.AddAll(indexes, oldData.IndexData.Keys);

            foreach (MemoryIndex index in indexes)
            {
                if (!DataEquals(oldData, index))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Simplifies memory entries which is bigget than simplify limit and compares data of this data collection object with the given one.
        /// </summary>
        /// <param name="oldData">The old data.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <param name="assistant">The assistant.</param>
        /// <returns>True whether compared data are the same.</returns>
        public bool DataEqualsAndSimplify(SnapshotData oldData, int simplifyLimit, MemoryAssistantBase assistant)
        {
            bool areEquals = true;

            HashSet<MemoryIndex> indexes = new HashSet<MemoryIndex>();
            HashSetTools.AddAll(indexes, this.IndexData.Keys);
            HashSetTools.AddAll(indexes, oldData.IndexData.Keys);

            foreach (MemoryIndex index in indexes)
            {
                if (!DataEqualsAndSimplify(oldData, index, simplifyLimit, assistant))
                {
                    areEquals = false;
                }
            }

            return areEquals;
        }

        /// <summary>
        /// Compares data of single memory index within this and given data container.
        /// </summary>
        /// <param name="oldData">The old data.</param>
        /// <param name="index">The index.</param>
        /// <returns>True whether compared data are the same.</returns>
        public bool DataEquals(SnapshotData oldData, MemoryIndex index)
        {
            MemoryEntry newEntry = null;
            if (!this.IndexData.TryGetValue(index, out newEntry))
            {
                newEntry = EmptyEntry;
            }

            MemoryEntry oldEntry = null;
            if (!oldData.IndexData.TryGetValue(index, out oldEntry))
            {
                oldEntry = EmptyEntry;
            }

            return oldEntry.Equals(newEntry);
        }


        /// <summary>
        /// Simplify data of single memory index if exceeds given simplify limit and compares it with the data inside given data container.
        /// </summary>
        /// <param name="oldData">The old data.</param>
        /// <param name="index">The index.</param>
        /// <param name="simplifyLimit">The simplify limit.</param>
        /// <param name="assistant">The assistant.</param>
        /// <returns>True whether compared data are the same.</returns>
        public bool DataEqualsAndSimplify(SnapshotData oldData, MemoryIndex index, int simplifyLimit, MemoryAssistantBase assistant)
        {
            MemoryEntry newEntry = null;
            if (!this.IndexData.TryGetValue(index, out newEntry))
            {
                newEntry = EmptyEntry;
            }

            MemoryEntry oldEntry = null;
            if (!oldData.IndexData.TryGetValue(index, out oldEntry))
            {
                oldEntry = EmptyEntry;
            }

            if (oldEntry.Equals(newEntry))
            {
                return true;
            }
            else if (newEntry.Count > simplifyLimit)
            {
                MemoryIndex testIndex = ControlIndex.Create(".return", 6);
                if (testIndex.Equals(index))
                {

                }

                MemoryEntry simplifiedEntry = assistant.Simplify(newEntry);
                SetMemoryEntry(index, simplifiedEntry);

                return oldEntry.Equals(simplifiedEntry);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Widens the data of given memory index in this data container.
        /// </summary>
        /// <param name="oldData">The old data.</param>
        /// <param name="index">The index.</param>
        /// <param name="assistant">The assistant.</param>
        public void DataWiden(SnapshotData oldData, MemoryIndex index, MemoryAssistantBase assistant)
        {
            MemoryEntry newEntry = null;
            if (!this.IndexData.TryGetValue(index, out newEntry))
            {
                newEntry = EmptyEntry;
            }

            MemoryEntry oldEntry = null;
            if (!oldData.IndexData.TryGetValue(index, out oldEntry))
            {
                oldEntry = EmptyEntry;
            }

            MemoryIndex testIndex = ControlIndex.Create(".return", 6);
            if (testIndex.Equals(index))
            {

            }

            if (!oldEntry.Equals(newEntry))
            {
                MemoryEntry widenedEntry = assistant.Widen(oldEntry, newEntry);

                CollectComposedValuesVisitor newVisitor = new CollectComposedValuesVisitor();
                newVisitor.VisitMemoryEntry(newEntry);

                CollectComposedValuesVisitor widenedVisitor = new CollectComposedValuesVisitor();
                widenedVisitor.VisitMemoryEntry(widenedEntry);

                if (newVisitor.Arrays.Count != widenedVisitor.Arrays.Count)
                {
                    snapshot.DestroyArray(index);
                }

                if (newVisitor.Objects.Count != widenedVisitor.Objects.Count)
                {
                    snapshot.Structure.SetObjects(index, null);
                }

                SetMemoryEntry(index, assistant.Widen(oldEntry, new MemoryEntry(widenedVisitor.Values)));
            }
        }

        /// <summary>
        /// Removes the undefined value from the entry of given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void RemoveUndefined(MemoryIndex index)
        {
            MemoryEntry oldEntry;
            if (IndexData.TryGetValue(index, out oldEntry))
            {
                HashSet<Value> values = new HashSet<Value>(oldEntry.PossibleValues);
                if (values.Contains(snapshot.UndefinedValue))
                {
                    values.Remove(snapshot.UndefinedValue);
                    SetMemoryEntry(index, new MemoryEntry(values));
                }
            }
        }
    }
}