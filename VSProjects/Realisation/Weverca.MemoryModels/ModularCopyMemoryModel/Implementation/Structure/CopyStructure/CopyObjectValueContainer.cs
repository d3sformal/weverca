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
    class CopyObjectValueContainer : CopySet<ObjectValue>, IObjectValueContainer, IObjectValueContainerBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectValueContainer"/> class.
        /// </summary>
        public CopyObjectValueContainer()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectValueContainer"/> class.
        /// </summary>
        /// <param name="objectContainer">The object container.</param>
        public CopyObjectValueContainer(CopyObjectValueContainer objectContainer)
            : base(objectContainer)
        {

        }

        /// <inheritdoc />
        public IObjectValueContainerBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyObjectValueContainer(this);
        }

        /// <inheritdoc />
        public new System.Collections.IEnumerator GetEnumerator()
        {
            return ((CopySet<ObjectValue>)this).GetEnumerator();
        }

        /// <inheritdoc />
        public IObjectValueContainer Build(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyObjectValueContainer(this);
        }
    }
}