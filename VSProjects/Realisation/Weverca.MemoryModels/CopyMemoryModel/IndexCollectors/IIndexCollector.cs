using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    interface IIndexCollector
    {
        IEnumerable<MemoryIndex> MustIndexes { get; }
        IEnumerable<MemoryIndex> MayIndexes { get; }

        void Next(Snapshot snapshot, PathSegment segment);
    }

    abstract class IndexCollector : IIndexCollector
    {
        public void ProcessPath(Snapshot snapshot, MemoryPath path)
        {
            foreach (PathSegment segment in path.PathSegments)
            {
                Next(snapshot, segment);
            }
        }

        public abstract IEnumerable<MemoryIndex> MustIndexes { get; }
        public abstract IEnumerable<MemoryIndex> MayIndexes { get; }
        public abstract void Next(Snapshot snapshot, PathSegment segment);
    }
}
