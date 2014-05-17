using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyMemoryAlias : IMemoryAlias, IMemoryAliasBuilder
    {
        private MemoryIndex sourceIndex;
        private CopySet<MemoryIndex> mayAliases;
        private CopySet<MemoryIndex> mustAliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyMemoryAlias"/> class.
        /// </summary>
        public CopyMemoryAlias()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyMemoryAlias"/> class.
        /// </summary>
        /// <param name="memoryAlias">The memory alias.</param>
        public CopyMemoryAlias(CopyMemoryAlias memoryAlias)
        {
            this.sourceIndex = memoryAlias.sourceIndex;
            this.mayAliases = memoryAlias.mayAliases.Clone();
            this.mustAliases = memoryAlias.mustAliases.Clone();
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
        public IMemoryAliasBuilder Builder()
        {
            return new CopyMemoryAlias(this);
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
        public IMemoryAlias Build()
        {
            return new CopyMemoryAlias(this);
        }
    }
}
