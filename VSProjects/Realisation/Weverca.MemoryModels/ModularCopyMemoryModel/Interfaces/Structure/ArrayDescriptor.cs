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
        /// <returns>New builder to modify this descriptor.</returns>
        IArrayDescriptorBuilder Builder();
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
        /// <returns>The imutable version of this collection.</returns>
        IArrayDescriptor Build();
    }
}
