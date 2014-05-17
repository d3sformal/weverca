using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>    
    /// Stores type and list of fields of PHP object. Every objects is determined by special value object
    /// with no content. This object is just pointer which is used to receive this descriptor instance when is need.
    /// This approach allows to use pointer object across whole memory model without need of modyfying structure
    /// when some new idnex is added - snapshot just simply modify descriptor instance and let the pointer to be
    /// still the same.
    /// 
    /// Imutable class. For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public interface IObjectDescriptor : IReadonlyIndexContainer
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        TypeValue Type { get; }

        /// <summary>
        /// Gets the object value representation of this object.
        /// </summary>
        /// <value>
        /// The object value.
        /// </value>
        ObjectValue ObjectValue { get; }

        /// <summary>
        /// Gets container builder to create new imutable instance with modified data.
        /// </summary>
        /// <returns>New builder to modify this descriptor.</returns>
        IObjectDescriptorBuilder Builder();
    }

    /// <summary>
    /// Mutable variant of ObjectDescriptor - use for creating new structure
    /// </summary>
    public interface IObjectDescriptorBuilder : IWriteableIndexContainer
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        TypeValue Type { get; }

        /// <summary>
        /// Gets the object value representation of this object.
        /// </summary>
        /// <value>
        /// The object value.
        /// </value>
        ObjectValue ObjectValue { get; }

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="type">The type.</param>
        void SetType(TypeValue type);

        /// <summary>
        /// Sets the objectvalue.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        void SetObjectValue(ObjectValue objectValue);

        /// <summary>
        /// Gets the imutable version of this collection.
        /// </summary>
        /// <returns>The imutable version of this collection.</returns>
        IObjectDescriptor Build();
    }
}
