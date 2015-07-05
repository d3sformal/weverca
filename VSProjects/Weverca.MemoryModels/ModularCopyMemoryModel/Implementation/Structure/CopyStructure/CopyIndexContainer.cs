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
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyIndexContainerFactory : IIndexContainerFactory
    {

        public IWriteableIndexContainer CreateWriteableIndexContainer()
        {
            return new CopyIndexContainer();
        }
    }

    /// <summary>
    /// Typycal implementation of index container which can be used to store variable names and their mapping
    /// to memory indexes.
    /// 
    /// This is NOT imutable class.
    /// </summary>
    public class CopyIndexContainer : IReadonlyIndexContainer, IWriteableIndexContainer, IGenericCloneable<CopyIndexContainer>
    {
        private Dictionary<string, MemoryIndex> indexes;
        private MemoryIndex unknownIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexContainer"/> class.
        /// </summary>
        public CopyIndexContainer()
        {
            indexes = new Dictionary<string, MemoryIndex>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexContainer"/> class.
        /// 
        /// Content of given container is copied to the new container.
        /// </summary>
        /// <param name="container">The container.</param>
        public CopyIndexContainer(IReadonlyIndexContainer container)
        {
            unknownIndex = container.UnknownIndex;

            indexes = new Dictionary<string, MemoryIndex>();
            CollectionMemoryUtils.AddAll(indexes, container.Indexes);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, MemoryIndex>> Indexes
        {
            get { return indexes; }
        }

        /// <inheritdoc />
        public int Count
        {
            get { return indexes.Count; }
        }

        /// <inheritdoc />
        public MemoryIndex UnknownIndex
        {
            get { return unknownIndex; }
        }

        /// <inheritdoc />
        public MemoryIndex GetIndex(string indexName)
        {
            MemoryIndex index;
            if (indexes.TryGetValue(indexName, out index))
            {
                return index;
            }
            else
            {
                throw new IndexOutOfRangeException("Collection doues not contain index with specified name.");
            }
        }

        /// <inheritdoc />
        public bool TryGetIndex(string indexName, out MemoryIndex index)
        {
            return indexes.TryGetValue(indexName, out index);
        }

        /// <inheritdoc />
        public bool ContainsIndex(string indexName)
        {
            return indexes.ContainsKey(indexName);
        }

        /// <inheritdoc />
        public void AddIndex(string indexName, MemoryIndex memoryIndex)
        {
            indexes[indexName] = memoryIndex;
        }

        /// <inheritdoc />
        public void RemoveIndex(string indexName)
        {
            indexes.Remove(indexName);
        }

        /// <inheritdoc />
        public void SetUnknownIndex(MemoryIndex unknownIndex)
        {
            this.unknownIndex = unknownIndex;
        }

        /// <inheritdoc />
        public CopyIndexContainer Clone()
        {
            return new CopyIndexContainer(this);
        }
    }
}