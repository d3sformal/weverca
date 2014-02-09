using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    public class MergeOperation
    {
        public readonly List<Tuple<MemoryIndex, Snapshot>> Indexes = new List<Tuple<MemoryIndex, Snapshot>>();

        public bool IsUndefined { get; private set; }
        public MemoryIndex TargetIndex { get; private set; }
        public bool IsRoot { get; set; }

        public MergeOperation(MemoryIndex targetIndex)
        {
            IsUndefined = false;
            TargetIndex = targetIndex;
        }

        public MergeOperation()
        {
            IsUndefined = false;
        }

        internal void Add(MemoryIndex memoryIndex, Snapshot snapshot)
        {
            if (memoryIndex == null)
            {
                throw new NullReferenceException();
            }

            Indexes.Add(new Tuple<MemoryIndex, Snapshot>(memoryIndex, snapshot));
        }

        internal void SetUndefined()
        {
            IsUndefined = true;
        }

        internal void SetTargetIndex(MemoryIndex targetIndex)
        {
            TargetIndex = targetIndex;
        }

        public override string ToString()
        {
            return TargetIndex.ToString();
        }
    }

    /*class MergeWithinSnapshotOperation
    {
        public readonly HashSet<MemoryIndex> Indexes = new HashSet<MemoryIndex>();

        public bool IsUndefined { get; private set; }
        public MemoryIndex TargetIndex { get; private set; }

        public MergeWithinSnapshotOperation(MemoryIndex targetIndex)
        {
            IsUndefined = false;
            TargetIndex = targetIndex;
        }

        public MergeWithinSnapshotOperation()
        {
            IsUndefined = false;
        }

        internal void Add(MemoryIndex memoryIndex)
        {
            Indexes.Add(memoryIndex);
        }

        internal void SetUndefined()
        {
            IsUndefined = true;
        }

        internal void SetTargetIndex(MemoryIndex targetIndex)
        {
            TargetIndex = targetIndex;
        }

        public bool IsRoot { get; set; }
    }*/
}
