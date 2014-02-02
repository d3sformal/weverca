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

        int MustIndexesCount { get; }
        int MayIndexesCount { get; }

        bool IsDefined { get; }

        void Next(PathSegment segment);
    }

    abstract class IndexCollector : IIndexCollector
    {
        public GlobalContext Global { get; private set; }
        public int CallLevel { get; private set; }

        public void ProcessPath(MemoryPath path)
        {
            Global = path.Global;
            CallLevel = path.CallLevel;

            foreach (PathSegment segment in path.PathSegments)
            {
                Next(segment);
            }
        }

        public abstract IEnumerable<MemoryIndex> MustIndexes { get; }
        public abstract IEnumerable<MemoryIndex> MayIndexes { get; }
        public abstract int MustIndexesCount { get; }
        public abstract int MayIndexesCount { get; }
        public abstract bool IsDefined { get; protected set; }
        public abstract void Next(PathSegment segment);
    }
}
