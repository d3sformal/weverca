using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    public interface ReadonlyIndexContainer
    {
        IReadOnlyDictionary<string, MemoryIndex> Indexes { get; }
        MemoryIndex UnknownIndex { get; }
    }

    public interface IWriteableIndexContainer
    {
        MemoryIndex UnknownIndex { get; }
        Dictionary<string, MemoryIndex> Indexes { get; }
    }

    public class IndexContainer : IWriteableIndexContainer, ReadonlyIndexContainer
    {
        public MemoryIndex UnknownIndex { get; private set; }
        public Dictionary<string, MemoryIndex> Indexes { get; private set; }

        public IndexContainer(MemoryIndex unknownIndex)
        {
            UnknownIndex = unknownIndex;
            Indexes = new Dictionary<string, MemoryIndex>();
        }

        public IndexContainer(IndexContainer indexContainer)
        {
            UnknownIndex = indexContainer.UnknownIndex;
            Indexes = new Dictionary<string, MemoryIndex>(indexContainer.Indexes);
        }

        IReadOnlyDictionary<string, MemoryIndex> ReadonlyIndexContainer.Indexes
        {
            get { return Indexes; }
        }
    }
}
