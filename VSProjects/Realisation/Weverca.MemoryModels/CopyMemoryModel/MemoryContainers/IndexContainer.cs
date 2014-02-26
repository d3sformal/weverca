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
    /// <summary>
    /// Represents special object which has semantics as imutable index container. This container contains
    /// imutable collection of memory indexes and their names and unknown index.
    /// 
    /// Each instance of this interface represents inner node of memory tree (array with indexes, object with fields,
    /// variable container).
    /// </summary>
    public interface ReadonlyIndexContainer
    {
        /// <summary>
        /// Gets the collection of indexes.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        IReadOnlyDictionary<string, MemoryIndex> Indexes { get; }

        /// <summary>
        /// Gets the speacial index which is used when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        MemoryIndex UnknownIndex { get; }
    }

    /// <summary>
    /// Mutable version of ReadonlyIndexContainer interface.
    /// 
    /// Represents special object which has semantics as index container. This container contains
    /// collection of memory indexes and their names and unknown index.
    /// 
    /// Each instance of this interface represents inner node of memory tree (array with indexes, object with fields,
    /// variable container).
    /// </summary>
    public interface IWriteableIndexContainer
    {
        /// <summary>
        /// Gets the speacial index which is used when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        MemoryIndex UnknownIndex { get; }

        /// <summary>
        /// Gets the collection of indexes.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        Dictionary<string, MemoryIndex> Indexes { get; }
    }

    /// <summary>
    /// Typycal implementation of index container which can be used to store variable names and their mapping
    /// to memory indexes.
    /// 
    /// This is NOT imutable class.
    /// </summary>
    public class IndexContainer : IWriteableIndexContainer, ReadonlyIndexContainer, IGenericCloneable<IndexContainer>
    {
        /// <summary>
        /// Gets the speacial index which is used when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        public MemoryIndex UnknownIndex { get; private set; }

        /// <summary>
        /// Gets the collection of indexes.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        public Dictionary<string, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexContainer"/> class.
        /// </summary>
        /// <param name="unknownIndex">Index of the unknown.</param>
        public IndexContainer(MemoryIndex unknownIndex)
        {
            UnknownIndex = unknownIndex;
            Indexes = new Dictionary<string, MemoryIndex>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexContainer"/> class by copying content from another container.
        /// </summary>
        /// <param name="indexContainer">The index container.</param>
        public IndexContainer(IndexContainer indexContainer)
        {
            UnknownIndex = indexContainer.UnknownIndex;
            Indexes = new Dictionary<string, MemoryIndex>(indexContainer.Indexes);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexContainer"/> class by copying content from another container.
        /// </summary>
        /// <param name="indexContainer">The index container.</param>
        public IndexContainer(ReadonlyIndexContainer indexContainer)
        {
            UnknownIndex = indexContainer.UnknownIndex;
            Indexes = new Dictionary<string, MemoryIndex>();

            foreach (var index in indexContainer.Indexes)
            {
                Indexes.Add(index.Key, index.Value);
            }
        }

        /// <summary>
        /// Gets the collection of indexes.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        IReadOnlyDictionary<string, MemoryIndex> ReadonlyIndexContainer.Indexes
        {
            get { return Indexes; }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>New instance which contains copy of this instance.</returns>
        public IndexContainer Clone()
        {
            return new IndexContainer(this);
        }

        /// <summary>
        /// Gets the string representation of the collection - dump of all indexes and theirs data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <returns>String representation of the collection.</returns>
        internal string GetRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();

            GetRepresentation(data, infos, result);

            return result.ToString();
        }

        /// <summary>
        /// Gets the string representation of the collection - dump of all indexes and theirs data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="result">The result.</param>
        internal void GetRepresentation(SnapshotData data, SnapshotData infos, StringBuilder result)
        {
            GetRepresentation(this, data, infos, result);
        }

        /// <summary>
        /// Gets the string representation of the collection - dump of all indexes and theirs data.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="result">The result.</param>
        internal static void GetRepresentation(ReadonlyIndexContainer container, SnapshotData data, SnapshotData infos, StringBuilder result)
        {
            GetIndexRepresentation(container.UnknownIndex, data, infos, result);

            foreach (var item in container.Indexes)
            {
                MemoryIndex index = item.Value;
                GetIndexRepresentation(index, data, infos, result);
            }
        }

        /// <summary>
        /// Gets the string representation of the collection - dump of all indexes and theirs data.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="result">The result.</param>
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
