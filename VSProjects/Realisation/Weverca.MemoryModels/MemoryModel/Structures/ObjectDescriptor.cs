using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel
{
    public class ObjectDescriptor
    {
        public ReadOnlyDictionary<ContainerIndex, MemoryIndex> Fields { get; private set; }

        public ReadOnlyCollection<MemoryIndex> MustReferences { get; private set; }
        public ReadOnlyCollection<MemoryIndex> MayReferences { get; private set; }
        public ReadOnlyCollection<TypeValue> Types { get; private set; }

        private Snapshot snapshot;

        public ObjectDescriptor(ObjectDescriptorBuilder objectDescriptorBuilder)
        {
            Fields = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(objectDescriptorBuilder.Fields);
            MustReferences = new ReadOnlyCollection<MemoryIndex>(objectDescriptorBuilder.MustReferences);
            MayReferences = new ReadOnlyCollection<MemoryIndex>(objectDescriptorBuilder.MayReferences);
            Types = new ReadOnlyCollection<TypeValue>(objectDescriptorBuilder.Types);
        }

        public ObjectDescriptor(TypeValue type, Snapshot snapshot)
        {
            this.snapshot = snapshot;
            Fields = new ReadOnlyDictionary<ContainerIndex, MemoryIndex>(new Dictionary<ContainerIndex, MemoryIndex>());

            MustReferences = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
            MayReferences = new ReadOnlyCollection<MemoryIndex>(new List<MemoryIndex>());
            Types = new ReadOnlyCollection<TypeValue>(new List<TypeValue>() { type });
        }

        public ObjectDescriptorBuilder Builder()
        {
            return new ObjectDescriptorBuilder(this);
        }
    }

    public class ObjectDescriptorBuilder
    {
        private ObjectDescriptor ObjectDescriptor;
        public MemoryIndex ParentVariable { get; set; }

        public Dictionary<ContainerIndex, MemoryIndex> Fields { get; private set; }
        public List<MemoryIndex> MustReferences { get; private set; }
        public List<MemoryIndex> MayReferences { get; private set; }
        public List<TypeValue> Types { get; private set; }

        public ObjectDescriptorBuilder(ObjectDescriptor objectDescriptor)
        {
            Fields = new Dictionary<ContainerIndex, MemoryIndex>(objectDescriptor.Fields);
            MustReferences = new List<MemoryIndex>(objectDescriptor.MustReferences);
            MayReferences = new List<MemoryIndex>(objectDescriptor.MayReferences);
            Types = new List<TypeValue>(objectDescriptor.Types);
        }

        public ObjectDescriptorBuilder add(ContainerIndex containerIndex, MemoryIndex fields)
        {
            Fields[containerIndex] = fields;
            return this;
        }

        public ObjectDescriptorBuilder addMustReference(MemoryIndex reference)
        {
            MustReferences.Add(reference);
            return this;
        }
        public ObjectDescriptorBuilder addMayReference(MemoryIndex reference)
        {
            MayReferences.Add(reference);
            return this;
        }
        public ObjectDescriptorBuilder removeMustReference(MemoryIndex reference)
        {
            MustReferences.Remove(reference);
            return this;
        }
        public ObjectDescriptorBuilder removeMayReference(MemoryIndex reference)
        {
            MayReferences.Remove(reference);
            return this;
        }

        public ObjectDescriptor Build()
        {
            return new ObjectDescriptor(this);
        }
    }
}
