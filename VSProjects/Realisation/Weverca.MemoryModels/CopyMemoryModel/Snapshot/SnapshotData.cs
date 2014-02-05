using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class SnapshotData
    {
        public readonly MemoryEntry EmptyEntry;

        static int DATA_ID = 0;
        int dataId = DATA_ID++;

        internal Dictionary<MemoryIndex, MemoryEntry> IndexData;
        Snapshot snapshot;

        private SnapshotData(Snapshot snapshot)
        {
            this.snapshot = snapshot;
            EmptyEntry = new MemoryEntry(snapshot.UndefinedValue);
        }

        public static SnapshotData CreateEmpty(Snapshot snapshot)
        {
            SnapshotData data = new SnapshotData(snapshot);

            data.IndexData = new Dictionary<MemoryIndex, MemoryEntry>();

            return data;
        }

        public SnapshotData Copy(Snapshot snapshot)
        {
            SnapshotData data = new SnapshotData(snapshot);

            data.IndexData = new Dictionary<MemoryIndex, MemoryEntry>(IndexData);

            return data;
        }



        #region MemoryEntries

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

        internal bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry)
        {
            return IndexData.TryGetValue(index, out entry);
        }

        internal void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry)
        {
            IndexData[index] = memoryEntry;
        }

        internal void RemoveMemoryEntry(MemoryIndex index)
        {
            IndexData.Remove(index);
        }
        #endregion

        internal bool DataEquals(SnapshotData oldData)
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

        internal bool DataEquals(SnapshotData oldData, MemoryIndex index)
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

        internal void DataWiden(SnapshotData oldData, MemoryIndex index, MemoryAssistantBase assistant)
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

            if (!oldEntry.Equals(newEntry))
            {
                SetMemoryEntry(index, assistant.Widen(oldEntry, newEntry));
            }
        }
    }
}
