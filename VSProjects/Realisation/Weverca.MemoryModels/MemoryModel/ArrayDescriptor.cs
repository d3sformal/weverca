using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    public class ArrayDescriptor
    {
        public ReadOnlyDictionary<ContainerIndex, MemoryIndex> Indexes { get; private set; }

        MemoryIndex parentVariable;
        private Snapshot snapshot;

        public ArrayDescriptor(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public ArrayDescriptor(ArrayDescriptorBuilder arrayDescriptorBuilder)
        {
            Indexes = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(arrayDescriptorBuilder.Indexes);
        }

        public ArrayDescriptorBuilder Builder()
        {
            return new ArrayDescriptorBuilder(this);
        }
    }

    public class ArrayDescriptorBuilder
    {
        private ArrayDescriptor arrayDescriptor;

        public Dictionary<ContainerIndex, MemoryIndex> Indexes { get; private set; }

        public ArrayDescriptorBuilder(ArrayDescriptor arrayDescriptor)
        {
            Indexes = new Dictionary<ContainerIndex, MemoryIndex>(arrayDescriptor.Indexes);
        }

        public ArrayDescriptorBuilder add(ContainerIndex containerIndex, MemoryIndex index)
        {
            Indexes[containerIndex] = index;
            return this;
        }

        public ArrayDescriptor Build()
        {
            return new ArrayDescriptor(this);
        }
    }
}
