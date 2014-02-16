using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    interface IIndexCollector
    {
        IEnumerable<MemoryIndex> MustIndexes { get; }
        IEnumerable<MemoryIndex> MayIndexes { get; }

        IEnumerable<CollectedLocation> MustLocation { get; }
        IEnumerable<CollectedLocation> MayLocaton { get; }

        int MustIndexesCount { get; }
        int MayIndexesCount { get; }

        bool IsDefined { get; }

        void Next(PathSegment segment);
    }

    class CollectedValue
    {
        public readonly HashSet<Value> Values = new HashSet<Value>();
        public readonly MemoryIndex ContainingIndex;
        public readonly Value OriginValue;

        public bool IsContainedInIndex { get { return ContainingIndex != null; } }
        public bool HasOriginValue { get { return OriginValue != null; } }

        public CollectedValue(MemoryIndex containingIndex)
        {
            ContainingIndex = containingIndex;
        }

        public CollectedValue(MemoryIndex containingIndex, Value originValue)
        {
            ContainingIndex = containingIndex;
            OriginValue = originValue;
        }

        public void AddValues(MemoryEntry entry)
        {
            HashSetTools.AddAll(Values, entry.PossibleValues);
        }

        public void AddValues(IEnumerable<Value> values)
        {
            HashSetTools.AddAll(Values, values);
        }
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
        public abstract IEnumerable<CollectedLocation> MustLocation { get; }
        public abstract IEnumerable<CollectedLocation> MayLocaton { get; }
        public abstract int MustIndexesCount { get; }
        public abstract int MayIndexesCount { get; }
        public abstract bool IsDefined { get; protected set; }
        public abstract void Next(PathSegment segment);
    }
}
