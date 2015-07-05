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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyMemoryAliasFactory : IMemoryAliasFactory
    {

        public IMemoryAlias CreateMemoryAlias(IWriteableSnapshotStructure targetStructure, MemoryIndex index)
        {
            CopyMemoryAlias aliases = new CopyMemoryAlias();
            aliases.SetSourceIndex(index);
            return aliases;
        }
    }

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
            mayAliases = new CopySet<MemoryIndex>();
            mustAliases = new CopySet<MemoryIndex>();
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
        public IMemoryAliasBuilder Builder(IWriteableSnapshotStructure targetStructure)
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
        public IMemoryAlias Build(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyMemoryAlias(this);
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