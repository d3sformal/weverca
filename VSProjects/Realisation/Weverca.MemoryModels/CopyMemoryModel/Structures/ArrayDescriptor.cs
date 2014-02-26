using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
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
    internal class ArrayDescriptor : ReadonlyIndexContainer
    {
        /// <summary>
        /// List of indexes for the array
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        public IReadOnlyDictionary<string, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Variable where the array is stored in
        /// </summary>
        /// <value>
        /// The parent variable.
        /// </value>
        public MemoryIndex ParentVariable { get; private set; }

        /// <summary>
        /// Speacial index when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        public MemoryIndex UnknownIndex { get; private set; }

        /// <summary>
        /// Pointing array value this descriptor is used for.
        /// </summary>
        /// <value>
        /// The array value.
        /// </value>
        public AssociativeArray ArrayValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <param name="parentVariable">The parent variable.</param>
        /// <exception cref="System.Exception">Null array in descriptor</exception>
        public ArrayDescriptor(AssociativeArray arrayValue, MemoryIndex parentVariable)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());
            ParentVariable = parentVariable;
            UnknownIndex = parentVariable.CreateUnknownIndex();
            ArrayValue = arrayValue;

            if (arrayValue == null)
            {
                throw new Exception("Null array in descriptor");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class.
        /// </summary>
        /// <param name="arrayDescriptorBuilder">The array descriptor builder.</param>
        public ArrayDescriptor(ArrayDescriptorBuilder arrayDescriptorBuilder)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(arrayDescriptorBuilder.Indexes);
            ParentVariable = arrayDescriptorBuilder.ParentVariable;
            UnknownIndex = arrayDescriptorBuilder.UnknownIndex;
            ArrayValue = arrayDescriptorBuilder.ArrayValue;
        }

        /// <summary>
        /// Creates new builder to modify this descriptor 
        /// </summary>
        /// <returns>New builder to modify this descriptor </returns>
        public ArrayDescriptorBuilder Builder()
        {
            return new ArrayDescriptorBuilder(this);
        }
    }

    /// <summary>
    /// Mutable variant of ArrayDescriptor - use for creating new structure
    /// </summary>
    internal class ArrayDescriptorBuilder : IWriteableIndexContainer
    {
        /// <summary>
        /// Gets or sets the variable where the array is stored in
        /// </summary>
        /// <value>
        /// The parent variable.
        /// </value>
        public MemoryIndex ParentVariable { get; set; }

        /// <summary>
        /// Gets the collection of indexes of array.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        public Dictionary<string, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Speacial index when the target location is unknown (ANY index)
        /// </summary>
        /// <value>
        /// The index of the unknown.
        /// </value>
        public MemoryIndex UnknownIndex { get; private set; }

        /// <summary>
        /// Pointing array value this descriptor is used for.
        /// </summary>
        /// <value>
        /// The array value.
        /// </value>
        public AssociativeArray ArrayValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptorBuilder"/> class from the given array descriptor.
        /// </summary>
        /// <param name="arrayDescriptor">The array descriptor.</param>
        public ArrayDescriptorBuilder(ArrayDescriptor arrayDescriptor)
        {
            IDictionary<string, MemoryIndex> collection = arrayDescriptor.Indexes as IDictionary<string, MemoryIndex>;

            if (collection != null)
            {
                Indexes = new Dictionary<string, MemoryIndex>(collection);
            }
            else
            {
                Indexes = new Dictionary<string, MemoryIndex>();
            }

            ParentVariable = arrayDescriptor.ParentVariable;
            UnknownIndex = arrayDescriptor.UnknownIndex;
            ArrayValue = arrayDescriptor.ArrayValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptorBuilder"/> class.
        /// </summary>
        public ArrayDescriptorBuilder()
        {
            Indexes = new Dictionary<string, MemoryIndex>();
            ArrayValue = null;
            ParentVariable = null;
        }

        /// <summary>
        /// Adds the specified container index.
        /// </summary>
        /// <param name="containerIndex">Index of the container.</param>
        /// <param name="index">The index.</param>
        /// <returns>This builder object.</returns>
        public ArrayDescriptorBuilder add(string containerIndex, MemoryIndex index)
        {
            Indexes[containerIndex] = index;
            return this;
        }

        /// <summary>
        /// Builds new descriptor object from this instance.
        /// </summary>
        /// <returns>New descriptor object from this instance.</returns>
        public ArrayDescriptor Build()
        {
            if (ArrayValue == null)
            {
                throw new Exception("Null array in descriptor");
            }

            return new ArrayDescriptor(this);
        }

        /// <summary>
        /// Sets the parent variable.
        /// </summary>
        /// <param name="parentIndex">Index of the parent.</param>
        /// <returns>This builder object.</returns>
        internal ArrayDescriptorBuilder SetParentVariable(MemoryIndex parentIndex)
        {
            ParentVariable = parentIndex;
            return this;
        }

        /// <summary>
        /// Sets the array value.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        /// <returns>This builder object.</returns>
        /// <exception cref="System.Exception">Null array in descriptor</exception>
        internal ArrayDescriptorBuilder SetArrayValue(AssociativeArray arrayValue)
        {
            if (arrayValue == null)
            {
                throw new Exception("Null array in descriptor");
            }

            ArrayValue = arrayValue;
            return this;
        }

        /// <summary>
        /// Sets the unknown field.
        /// </summary>
        /// <param name="memoryIndex">Index of the memory.</param>
        /// <returns>This builder object.</returns>
        internal ArrayDescriptorBuilder SetUnknownField(MemoryIndex memoryIndex)
        {
            UnknownIndex = memoryIndex;
            return this;
        }
    }
}
