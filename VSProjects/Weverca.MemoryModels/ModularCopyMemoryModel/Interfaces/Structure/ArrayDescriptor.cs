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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Instances of this factory class are used to create the new empty object
    /// which implements IArrayDescriptor.
    /// </summary>
    public interface IArrayDescriptorFactory
    {
        /// <summary>
        /// Creates the new instance of array descriptor to store array definition in structure.
        /// </summary>
        /// <param name="targetStructure">The target structure.</param>
        /// <param name="createdArray">The created array.</param>
        /// <param name="memoryIndex">The memory location of array.</param>
        /// <returns>
        /// Created array descriptor instance.
        /// </returns>
        IArrayDescriptor CreateArrayDescriptor(IWriteableSnapshotStructure targetStructure, AssociativeArray createdArray, MemoryIndex memoryIndex);
    }

    /// <summary>
    /// Stores list of indexes of associative array. Every associative array is determined by special value object
    /// with no content. This object is just pointer which is used to receive this descriptor instance when is need.
    /// This approach allows to use pointer object across whole memory model without need of modyfying structure
    /// when some new idnex is added - snapshot just simply modify descriptor instance and let the pointer to be
    /// still the same.
    /// 
    /// Imutable class. For modification use builder object 
    ///     descriptor.Builder().modify().Build()
    /// </summary>
    public interface IArrayDescriptor : IReadonlyIndexContainer
    {
        /// <summary>
        /// Gets the index of the memory location where this array is stored in.
        /// </summary>
        /// <value>
        /// The index of the memory location where this array is stored in.
        /// </value>
        MemoryIndex ParentIndex { get; }

        /// <summary>
        /// Gets the array value representation of this array.
        /// </summary>
        /// <value>
        /// The array value.
        /// </value>
        AssociativeArray ArrayValue { get; }

        /// <summary>
        /// Gets container builder to create new imutable instance with modified data.
        /// </summary>
        /// <param name="targetStructure">The structure object for which a builder created.</param>
        /// <returns>
        /// New builder to modify this descriptor.
        /// </returns>
        IArrayDescriptorBuilder Builder(IWriteableSnapshotStructure targetStructure);
    }

    /// <summary>
    /// Mutable variant of ArrayDescriptor - use for creating new structure
    /// </summary>
    public interface IArrayDescriptorBuilder : IReadonlyIndexContainer, IWriteableIndexContainer
    {
        /// <summary>
        /// Gets the index of the memory location where this array is stored in.
        /// </summary>
        /// <value>
        /// The index of the memory location where this array is stored in.
        /// </value>
        MemoryIndex ParentIndex { get; }

        /// <summary>
        /// Gets the array value representation of this array.
        /// </summary>
        /// <value>
        /// The array value.
        /// </value>
        AssociativeArray ArrayValue { get; }

        /// <summary>
        /// Sets the parent index value.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        void SetParentIndex(MemoryIndex parentIndex);

        /// <summary>
        /// Sets the array value.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        void SetArrayValue(AssociativeArray arrayValue);

        /// <summary>
        /// Gets the imutable version of this collection.
        /// </summary>
        /// <param name="targetStructure">The structure object for which is the instance built.</param>
        /// <returns>
        /// The imutable version of this collection.
        /// </returns>
        IArrayDescriptor Build(IWriteableSnapshotStructure targetStructure);
    }
}