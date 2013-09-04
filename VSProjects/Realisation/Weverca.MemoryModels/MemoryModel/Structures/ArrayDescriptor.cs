using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    /// <summary>
    /// Stores information about array value - indexes of the array
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public class ArrayDescriptor
    {
        /// <summary>
        /// List of indexes for the array
        /// </summary>
        public ReadOnlyDictionary<ContainerIndex, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Variable where the array is stored in
        /// </summary>
        public ReadOnlyCollection<MemoryIndex> ParentVariable { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class.
        /// </summary>
        public ArrayDescriptor()
        {
            Indexes = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(new Dictionary<ContainerIndex, MemoryIndex>());
            ParentVariable = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptor"/> class from builder object.
        /// </summary>
        /// <param name="arrayDescriptorBuilder">The array descriptor builder.</param>
        public ArrayDescriptor(ArrayDescriptorBuilder arrayDescriptorBuilder)
        {
            Indexes = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(arrayDescriptorBuilder.Indexes);
            ParentVariable = new ReadOnlyCollection<MemoryIndex>(arrayDescriptorBuilder.ParentVariable);
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
    public class ArrayDescriptorBuilder
    {
        /// <summary>
        /// List of variables where the array is stored in
        /// </summary>
        public List<MemoryIndex> ParentVariable { get; set; }

        /// <summary>
        /// List of indexes for the array
        /// </summary>
        public Dictionary<ContainerIndex, MemoryIndex> Indexes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayDescriptorBuilder"/> class from the given array descriptor.
        /// </summary>
        /// <param name="arrayDescriptor">The array descriptor.</param>
        public ArrayDescriptorBuilder(ArrayDescriptor arrayDescriptor)
        {
            Indexes = new Dictionary<ContainerIndex, MemoryIndex>(arrayDescriptor.Indexes);
            ParentVariable = new List<MemoryIndex>(arrayDescriptor.ParentVariable);
        }

        /// <summary>
        /// Adds the specified container index.
        /// </summary>
        /// <param name="containerIndex">Index of the container.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public ArrayDescriptorBuilder add(ContainerIndex containerIndex, MemoryIndex index)
        {
            Indexes[containerIndex] = index;
            return this;
        }

        /// <summary>
        /// Adds the parent variable.
        /// </summary>
        /// <param name="parentVariable">The parent variable.</param>
        /// <returns></returns>
        internal ArrayDescriptorBuilder AddParentVariable(MemoryIndex parentVariable)
        {
            ParentVariable.Add(parentVariable);
            return this;
        }

        /// <summary>
        /// Removes the parent variable.
        /// </summary>
        /// <param name="parentVariable">The parent variable.</param>
        /// <returns></returns>
        internal ArrayDescriptorBuilder RemoveParentVariable(MemoryIndex parentVariable)
        {
            ParentVariable.Remove(parentVariable);
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
    }
}
