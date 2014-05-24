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
        /// Initializes a new instance of the <see cref="SnapshotDataAssociativeContainer"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public SnapshotDataAssociativeContainer(Snapshot snapshot)
            : base(snapshot)
        {
            IndexData = new Dictionary<MemoryIndex, MemoryEntry>();
        }

        /// <summary>
        /// Creates new data instance and copies data from this collection to the new one.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New data instance and copies data from this collection to the new one.</returns>
        public SnapshotDataAssociativeContainer Copy(Snapshot snapshot)
        {
            SnapshotDataAssociativeContainer data = new SnapshotDataAssociativeContainer(snapshot);

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
            IndexData[index] = memoryEntry;
        }

        /// <inheritdoc />
        public override void RemoveMemoryEntry(MemoryIndex index)
        {
            IndexData.Remove(index);
        }
    }
}
