using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    class LazyCopyMemoryAliasFactory : IMemoryAliasFactory
    {
        public IMemoryAlias CreateMemoryAlias(IWriteableSnapshotStructure targetStructure, MemoryIndex index)
        {
            LazyCopyMemoryAlias aliases = new LazyCopyMemoryAlias(targetStructure);
            aliases.SetSourceIndex(index);
            return aliases;
        }
    }


    class LazyCopyMemoryAlias : IMemoryAlias, IMemoryAliasBuilder
    {
        private MemoryIndex sourceIndex;
        private LazyCopySet<MemoryIndex> mayAliases;
        private LazyCopySet<MemoryIndex> mustAliases;
        private IWriteableSnapshotStructure associatedStructure;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyMemoryAlias"/> class.
        /// </summary>
        public LazyCopyMemoryAlias(IWriteableSnapshotStructure associatedStructure)
        {
            mayAliases = new LazyCopySet<MemoryIndex>();
            mustAliases = new LazyCopySet<MemoryIndex>();
            this.associatedStructure = associatedStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyMemoryAlias"/> class.
        /// </summary>
        /// <param name="memoryAlias">The memory alias.</param>
        public LazyCopyMemoryAlias(IWriteableSnapshotStructure associatedStructure, LazyCopyMemoryAlias memoryAlias)
        {
            this.sourceIndex = memoryAlias.sourceIndex;
            this.mayAliases = memoryAlias.mayAliases.Clone();
            this.mustAliases = memoryAlias.mustAliases.Clone();
            this.associatedStructure = associatedStructure;
        }

        /// <inheritdoc />
        public MemoryIndex SourceIndex
        {
            get { return sourceIndex; }
        }

        /// <inheritdoc />
        public IReadonlySet<MemoryIndex> MayAliases
        {
            get { return mayAliases; }
        }

        /// <inheritdoc />
        public IReadonlySet<MemoryIndex> MustAliases
        {
            get { return mustAliases; }
        }

        /// <inheritdoc />
        public IMemoryAliasBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyMemoryAlias(targetStructure, this);
            }
        }

        /// <inheritdoc />
        IWriteableSet<MemoryIndex> IMemoryAliasBuilder.MayAliases
        {
            get { return mayAliases; }
        }

        /// <inheritdoc />
        IWriteableSet<MemoryIndex> IMemoryAliasBuilder.MustAliases
        {
            get { return mustAliases; }
        }

        /// <inheritdoc />
        public void SetSourceIndex(MemoryIndex index)
        {
            sourceIndex = index;
        }

        /// <inheritdoc />
        public IMemoryAlias Build(IWriteableSnapshotStructure targetStructure)
        {
            if (targetStructure == associatedStructure)
            {
                return this;
            }
            else
            {
                return new LazyCopyMemoryAlias(targetStructure, this);
            }
        }


        /// <inheritdoc />
        public bool HasAliases
        {
            get { return mustAliases.Count > 0 || mayAliases.Count > 0; }
        }

        /// <inheritdoc />
        public bool HasMustAliases
        {
            get { return mustAliases.Count > 0; }
        }
    }
}
