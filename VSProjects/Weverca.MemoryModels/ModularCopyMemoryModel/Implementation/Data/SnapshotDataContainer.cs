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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data
{
    /// <summary>
    /// Contains data for all definned memory location in memory snapshot.
    /// 
    /// Implemented as associative array which maps memory indexes to memory entries with data.
    /// </summary>
    public class SnapshotDataAssociativeContainer : AbstractSnapshotData
    {
        /// <summary>
        /// Associative container with memory entries for all memory locations.
        /// </summary>
        private Dictionary<MemoryIndex, MemoryEntry> IndexData;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotDataAssociativeContainer" /> class.
        /// </summary>
        public SnapshotDataAssociativeContainer()
            : base()
        {
            IndexData = new Dictionary<MemoryIndex, MemoryEntry>();
        }

        /// <summary>
        /// Creates new data instance and copies data from this collection to the new one.
        /// </summary>
        /// <returns>
        /// New data instance and copies data from this collection to the new one.
        /// </returns>
        public SnapshotDataAssociativeContainer Copy()
        {
            SnapshotDataAssociativeContainer data = new SnapshotDataAssociativeContainer();

            data.IndexData = new Dictionary<MemoryIndex, MemoryEntry>(IndexData);

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
        public override bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            return IndexData.TryGetValue(index, out entry);
        }

        /// <inheritdoc />
        public override void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            IndexData[index] = memoryEntry;
        }

        /// <inheritdoc />
        public override void RemoveMemoryEntry(MemoryIndex index)
        {
            IndexData.Remove(index);
        }
    }
}