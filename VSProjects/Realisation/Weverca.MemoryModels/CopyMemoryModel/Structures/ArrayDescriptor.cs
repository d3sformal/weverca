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
    /// Stores information about array value - indexes of the array
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    internal class ArrayDescriptor : ReadonlyIndexContainer
    {
        /// <summary>
        /// List of indexes for the array
        /// </summary>
        public IReadOnlyDictionary<string, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Variable where the array is stored in
        /// </summary>
        public MemoryIndex ParentVariable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public MemoryIndex UnknownIndex { get; private set; }

        public AssociativeArray ArrayValue { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class.
        /// </summary>
        public ArrayDescriptor(AssociativeArray arrayValue)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());
            ArrayValue = arrayValue;
            ParentVariable = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class.
        /// </summary>
        /// <param name="parentVariable">The parent variable.</param>
        public ArrayDescriptor(MemoryIndex parentVariable)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());
            ParentVariable = parentVariable;
            UnknownIndex = parentVariable.CreateUnknownIndex();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class from builder object.
        /// </summary>
        /// <param name="arrayDescriptorBuilder">The array descriptor builder.</param>
        public ArrayDescriptor(ArrayDescriptorBuilder arrayDescriptorBuilder)
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(arrayDescriptorBuilder.Indexes);
            ParentVariable = arrayDescriptorBuilder.ParentVariable;
            UnknownIndex = arrayDescriptorBuilder.UnknownIndex;
            ArrayValue = arrayDescriptorBuilder.ArrayValue;
        }

        public ArrayDescriptor()
        {
            Indexes = new ReadOnlyDictionary<string, MemoryIndex>(new Dictionary<string, MemoryIndex>());
            ArrayValue = null;
            ParentVariable = null;
        }

        /// <summary>
        /// Creates new builder to modify this descriptor 
        /// </summary>
        /// <returns></returns>
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
        /// List of variables where the array is stored in
        /// </summary>
        public MemoryIndex ParentVariable { get; set; }

        /// <summary>
        /// List of indexes for the array
        /// </summary>
        public Dictionary<string, MemoryIndex> Indexes { get; private set; }
       
        /// <summary>
        /// 
        /// </summary>
        public MemoryIndex UnknownIndex { get; private set; }

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
        /// Adds the specified container index.
        /// </summary>
        /// <param name="containerIndex">Index of the container.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ArrayDescriptorBuilder add(string containerIndex, MemoryIndex index)
        {
            Indexes[containerIndex] = index;
            return this;
        }

        /// <summary>
        /// Builds new descriptor object from this instance.
        /// </summary>
        /// <returns></returns>
        public ArrayDescriptor Build()
        {
            return new ArrayDescriptor(this);
        }

        internal ArrayDescriptorBuilder SetParentVariable(MemoryIndex parentIndex)
        {
            ParentVariable = parentIndex;
            return this;
        }

        internal ArrayDescriptorBuilder SetArrayValue(AssociativeArray arrayValue)
        {
            ArrayValue = arrayValue;
            return this;
        }

        internal ArrayDescriptorBuilder SetUnknownField(MemoryIndex memoryIndex)
        {
            UnknownIndex = memoryIndex;
            return this;
        }
    }
}
