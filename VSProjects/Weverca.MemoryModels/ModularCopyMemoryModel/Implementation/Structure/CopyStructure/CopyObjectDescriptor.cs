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
    class CopyObjectDescriptor : CopyIndexContainer, IObjectDescriptor, IObjectDescriptorBuilder
    {
        private TypeValue type;
        private ObjectValue objectValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// </summary>
        public CopyObjectDescriptor()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="objectValue">The object value.</param>
        public CopyObjectDescriptor(TypeValue type, ObjectValue objectValue)
        {
            this.type = type;
            this.objectValue = objectValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyObjectDescriptor"/> class.
        /// New instance contains copy of given object.
        /// </summary>
        /// <param name="objectDescriptor">The object descriptor.</param>
        public CopyObjectDescriptor(CopyObjectDescriptor objectDescriptor)
            : base(objectDescriptor)
        {
            this.type = objectDescriptor.type;
            this.objectValue = objectDescriptor.objectValue;
        }

        /// <inheritdoc />
        public TypeValue Type
        {
            get { return type; }
        }

        /// <inheritdoc />
        public ObjectValue ObjectValue
        {
            get { return objectValue; }
        }

        /// <inheritdoc />
        public IObjectDescriptorBuilder Builder(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyObjectDescriptor(this);
        }


        /// <inheritdoc />
        public void SetType(TypeValue type)
        {
            this.type = type;
        }

        /// <inheritdoc />
        public void SetObjectValue(ObjectValue objectValue)
        {
            this.objectValue = objectValue;
        }

        /// <inheritdoc />
        public IObjectDescriptor Build(IWriteableSnapshotStructure targetStructure)
        {
            return new CopyObjectDescriptor(this);
        }
    }
}