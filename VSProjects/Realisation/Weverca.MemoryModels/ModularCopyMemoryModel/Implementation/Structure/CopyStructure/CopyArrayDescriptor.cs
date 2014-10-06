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
    class CopyArrayDescriptor : CopyIndexContainer, IArrayDescriptor, IArrayDescriptorBuilder
    {
        private MemoryIndex parentIndex;
        private AssociativeArray arrayValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyArrayDescriptor"/> class.
        /// </summary>
        public CopyArrayDescriptor()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyArrayDescriptor"/> class.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <param name="arrayValue">The array value.</param>
        public CopyArrayDescriptor(MemoryIndex parentIndex, AssociativeArray arrayValue)
        {
            this.parentIndex = parentIndex;
            this.arrayValue = arrayValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyArrayDescriptor"/> class.
        /// New instance contains copy of given object.
        /// </summary>
        /// <param name="arrayDescriptor">The array descriptor.</param>
        public CopyArrayDescriptor(CopyArrayDescriptor arrayDescriptor)
            : base(arrayDescriptor)
        {
            this.parentIndex = arrayDescriptor.parentIndex;
            this.arrayValue = arrayDescriptor.arrayValue;
        }

        /// <inheritdoc />
        public MemoryIndex ParentIndex
        {
            get { return parentIndex; }
        }

        /// <inheritdoc />
        public AssociativeArray ArrayValue
        {
            get { return arrayValue; }
        }

        /// <inheritdoc />
        public IArrayDescriptorBuilder Builder()
        {
            return new CopyArrayDescriptor(this);
        }

        /// <inheritdoc />
        public void SetParentIndex(MemoryIndex parentIndex)
        {
            this.parentIndex = parentIndex;
        }

        /// <inheritdoc />
        public void SetArrayValue(AssociativeArray arrayValue)
        {
            this.arrayValue = arrayValue;
        }

        /// <inheritdoc />
        public IArrayDescriptor Build()
        {
            return new CopyArrayDescriptor(this);
        }
    }
}