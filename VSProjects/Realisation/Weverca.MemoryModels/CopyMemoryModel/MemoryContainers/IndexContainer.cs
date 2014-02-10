using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

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

    public class IndexContainer : IWriteableIndexContainer, ReadonlyIndexContainer, IGenericCloneable<IndexContainer>
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

        public IndexContainer(ReadonlyIndexContainer indexContainer)
        {
            UnknownIndex = indexContainer.UnknownIndex;
            Indexes = new Dictionary<string, MemoryIndex>();

            foreach (var index in indexContainer.Indexes)
            {
                Indexes.Add(index.Key, index.Value);
            }
        }

        IReadOnlyDictionary<string, MemoryIndex> ReadonlyIndexContainer.Indexes
        {
            get { return Indexes; }
        }

        public IndexContainer Clone()
        {
            return new IndexContainer(this);
        }

        internal string GetRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();

            GetRepresentation(data, infos, result);

            return result.ToString();
        }

        internal void GetRepresentation(SnapshotData data, SnapshotData infos, StringBuilder result)
        {
            GetRepresentation(this, data, infos, result);
        }

        internal static void GetRepresentation(ReadonlyIndexContainer container, SnapshotData data, SnapshotData infos, StringBuilder result)
        {
            GetIndexRepresentation(container.UnknownIndex, data, infos, result);

            foreach (var item in container.Indexes)
            {
                MemoryIndex index = item.Value;
                GetIndexRepresentation(index, data, infos, result);
            }
        }

        internal static void GetIndexRepresentation(MemoryIndex index, SnapshotData data, SnapshotData infos, StringBuilder result)
        {
            result.AppendFormat("{0}: {{ ", index);

            MemoryEntry dataEntry, infoEntry;
            if (data.TryGetMemoryEntry(index, out dataEntry))
            {
                result.Append(dataEntry.ToString());
            }

            if (infos.TryGetMemoryEntry(index, out infoEntry))
            {
                result.Append(" INFO: ");
                result.Append(infoEntry.ToString());
            }
            result.AppendLine(" }");
        }
    }
}
