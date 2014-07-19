using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyCopyStructure
{
    class LazyCopyIndexContainer: IReadonlyIndexContainer, IWriteableIndexContainer, IGenericCloneable<LazyCopyIndexContainer>
    {
        private Dictionary<string, MemoryIndex> indexes;
        private MemoryIndex unknownIndex;
        private bool copied;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexContainer"/> class.
        /// </summary>
        public LazyCopyIndexContainer()
        {
            indexes = new Dictionary<string, MemoryIndex>();
            copied = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexContainer"/> class.
        /// 
        /// Content of given container is copied to the new container.
        /// </summary>
        /// <param name="container">The container.</param>
        public LazyCopyIndexContainer(IReadonlyIndexContainer container)
        {
            unknownIndex = container.UnknownIndex;

            indexes = new Dictionary<string, MemoryIndex>();
            CollectionTools.AddAll(indexes, container.Indexes);
            copied = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexContainer"/> class.
        /// 
        /// Content of given container is copied to the new container.
        /// </summary>
        /// <param name="container">The container.</param>
        public LazyCopyIndexContainer(LazyCopyIndexContainer container)
        {
            unknownIndex = container.UnknownIndex;

            indexes = container.indexes;
            copied = false;
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
            copy();
            indexes[indexName] = memoryIndex;
        }

        /// <inheritdoc />
        public void RemoveIndex(string indexName)
        {
            copy();
            indexes.Remove(indexName);
        }

        /// <inheritdoc />
        public void SetUnknownIndex(MemoryIndex unknownIndex)
        {
            this.unknownIndex = unknownIndex;
        }

        /// <inheritdoc />
        public LazyCopyIndexContainer Clone()
        {
            return new LazyCopyIndexContainer(this);
        }

        private void copy()
        {
            if (!copied)
            {
                indexes = new Dictionary<string, MemoryIndex>(indexes);
                copied = true;
            }
        }
    }
}
