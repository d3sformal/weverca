using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Represents special object which has semantics as imutable index container. This container contains
    /// imutable collection of memory indexes and their names and unknown index.
    /// 
    /// Each instance of this interface represents inner node of memory tree (array with indexes, object with fields,
    /// variable container).
    /// </summary>
    public interface IReadonlyIndexContainer
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

        void AddIndex(string variableName, MemoryIndex variableIndex);


    }
}
