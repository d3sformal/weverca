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
    /// Stores information about array value
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
        public MemoryIndex ParentVariable { get; private set; }

        public ArrayDescriptor()
        {
            Indexes = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(new Dictionary<ContainerIndex, MemoryIndex>());
        }

        public ArrayDescriptor(ArrayDescriptorBuilder arrayDescriptorBuilder)
        {
            Indexes = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(arrayDescriptorBuilder.Indexes);
            ParentVariable = arrayDescriptorBuilder.ParentVariable;
        }

        public ArrayDescriptorBuilder Builder()
        {
            return new ArrayDescriptorBuilder(this);
        }
    }

    public class ArrayDescriptorBuilder
    {
        private ArrayDescriptor arrayDescriptor;
        public MemoryIndex ParentVariable { get; set; }

        public Dictionary<ContainerIndex, MemoryIndex> Indexes { get; private set; }

        public ArrayDescriptorBuilder(ArrayDescriptor arrayDescriptor)
        {
            Indexes = new Dictionary<ContainerIndex, MemoryIndex>(arrayDescriptor.Indexes);
            ParentVariable = arrayDescriptor.ParentVariable;
        }

        public ArrayDescriptorBuilder add(ContainerIndex containerIndex, MemoryIndex index)
        {
            Indexes[containerIndex] = index;
            return this;
        }

        internal ArrayDescriptorBuilder SetParentVariable(MemoryIndex parentVariable)
        {
            ParentVariable = parentVariable;
            return this;
        }

        public ArrayDescriptor Build()
        {
            return new ArrayDescriptor(this);
        }
    }
}
