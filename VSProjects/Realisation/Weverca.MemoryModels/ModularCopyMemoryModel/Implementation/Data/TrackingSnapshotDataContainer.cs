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
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data
{
    /// <summary>
    /// Contains data for all definned memory location in memory snapshot.
    /// 
    /// Implemented as associative array which maps memory indexes to memory entries with data.
    /// </summary>
    public class TrackingSnapshotDataAssociativeContainer : AbstractSnapshotData
    {
        /// <summary>
        /// Associative container with memory entries for all memory locations.
        /// </summary>
        private Dictionary<MemoryIndex, MemoryEntry> IndexData;

        private ChangeTracker<MemoryIndex, IReadOnlySnapshotData> tracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotDataAssociativeContainer"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public TrackingSnapshotDataAssociativeContainer(Snapshot snapshot)
            : base(snapshot)
        {
            IndexData = new Dictionary<MemoryIndex, MemoryEntry>();
            tracker = new ChangeTracker<MemoryIndex, IReadOnlySnapshotData>(DataId, this, null);
        }

        /// <summary>
        /// Creates new data instance and copies data from this collection to the new one.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New data instance and copies data from this collection to the new one.</returns>
        public TrackingSnapshotDataAssociativeContainer Copy(Snapshot snapshot)
        {
            TrackingSnapshotDataAssociativeContainer data = new TrackingSnapshotDataAssociativeContainer(snapshot);

            data.IndexData = new Dictionary<MemoryIndex, MemoryEntry>(IndexData);
            data.tracker = new ChangeTracker<MemoryIndex, IReadOnlySnapshotData>(data.DataId, data, this.tracker);

            return data;
        }

        /// <inheritdoc />
        public override IEnumerable<MemoryIndex> Indexes
        {
            get { return IndexData.Keys; }
        }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<MemoryIndex, MemoryEntry>> Data
        {
            get { return IndexData; }
        }

        /// <inheritdoc />
        public override MemoryEntry GetMemoryEntry(MemoryIndex index)
        {
            MemoryEntry memoryEntry;
            if (TryGetMemoryEntry(index, out memoryEntry))
            {
                return memoryEntry;
            }
            else
            {
                return Snapshot.EmptyEntry;
            }
        }

        /// <inheritdoc />
        public override bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            return IndexData.TryGetValue(index, out entry);
        }

        /// <inheritdoc />
        public override void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            if (IndexData.ContainsKey(index))
            {
                tracker.Modified(index);
            }
            else
            {
                tracker.Inserted(index);
            }

            IndexData[index] = memoryEntry;
            
        }

        /// <inheritdoc />
        public override void RemoveMemoryEntry(MemoryIndex index)
        {
            if (IndexData.ContainsKey(index))
            {
                tracker.Deleted(index);
                IndexData.Remove(index);
            }
        }

        /// <inheritdoc />
        public override IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotData> IndexChangeTracker
        {
            get
            {
                return tracker;
            }
        }
    }
}