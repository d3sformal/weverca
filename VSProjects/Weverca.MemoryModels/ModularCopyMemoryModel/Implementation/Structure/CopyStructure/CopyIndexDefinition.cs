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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyIndexDefinitionFactory : IIndexDefinitionFactory
    {

        public IIndexDefinition CreateIndexDefinition(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyIndexDefinition();
        }
    }

    /// <summary>
    /// Copy implementation of index definition. Creation of builder creates new copy all the time.
    /// </summary>
    class CopyIndexDefinition : IIndexDefinition, IIndexDefinitionBuilder
    {
        private IMemoryAlias aliases;
        private IObjectValueContainer objects;
        private AssociativeArray arrayValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexDefinition"/> class.
        /// </summary>
        public CopyIndexDefinition()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyIndexDefinition"/> class.
        /// </summary>
        /// <param name="indexDefinition">The index definition.</param>
        public CopyIndexDefinition(CopyIndexDefinition indexDefinition)
        {
            this.aliases = indexDefinition.aliases;
            this.objects = indexDefinition.objects;
            this.arrayValue = indexDefinition.arrayValue;
        }

        /// <inheritdoc />
        public IMemoryAlias Aliases
        {
            get { return aliases; }
        }

        /// <inheritdoc />
        public IObjectValueContainer Objects
        {
            get { return objects; }
        }

        /// <inheritdoc />
        public AssociativeArray Array
        {
            get { return arrayValue; }
        }

        /// <inheritdoc />
        public IIndexDefinitionBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyIndexDefinition(this);
        }

        /// <inheritdoc />
        public void SetArray(AssociativeArray arrayValue)
        {
            this.arrayValue = arrayValue;
        }

        /// <inheritdoc />
        public void SetObjects(IObjectValueContainer objects)
        {
            this.objects = objects;
        }

        /// <inheritdoc />
        public void SetAliases(IMemoryAlias aliases)
        {
            this.aliases = aliases;
        }

        /// <inheritdoc />
        public IIndexDefinition Build(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyIndexDefinition(this);
        }
    }
}