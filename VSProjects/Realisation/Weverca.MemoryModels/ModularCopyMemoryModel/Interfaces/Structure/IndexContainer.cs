/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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
        IEnumerable<KeyValuePair<string, MemoryIndex>> Indexes { get; }

        /// <summary>
        /// Gets the numer of indexes in collection.
        /// </summary>
        /// <value>
        /// The count of indexes.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the speacial index which is used when the target location is unknown (ANY index)
        /// 
        /// All indexes which are not defined should read values from this location.
        /// </summary>
        /// <value>
        /// The index of the unknown location.
        /// </value>
        MemoryIndex UnknownIndex { get; }

        /// <summary>
        /// Gets the memory index which points to memory location defined by given name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>Memory index which points to memory location defined by given name.</returns>
        MemoryIndex GetIndex(String indexName);

        /// <summary>
        /// Tries to get the memory index which points to memory location defined by given name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="index">The memory index.</param>
        /// <returns>True whether container contains memory index for the given name.</returns>
        bool TryGetIndex(string indexName, out MemoryIndex index);

        /// <summary>
        /// Determines whether the container contains the memory index specified index name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>True whether the container contains the memory index specified index name.</returns>
        bool ContainsIndex(string indexName);
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
    public interface IWriteableIndexContainer : IReadonlyIndexContainer
    {
        /// <summary>
        /// Adds the the memory index which points to memory location defined by given name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="memoryIndex">The memory index.</param>
        void AddIndex(string indexName, MemoryIndex memoryIndex);

        /// <summary>
        /// Removes the the memory index which points to memory location defined by given name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        void RemoveIndex(string indexName);

        /// <summary>
        /// Sets the the memory index for unknown location of this container.
        /// 
        /// All indexes which are not defined should read values from this location.
        /// </summary>
        /// <param name="unknownIndex">Index of the unknown location.</param>
        void SetUnknownIndex(MemoryIndex unknownIndex);
    }
}