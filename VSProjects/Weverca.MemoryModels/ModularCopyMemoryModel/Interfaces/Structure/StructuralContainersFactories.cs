using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    public class StructuralContainersFactories
    {

        public IArrayDescriptorFactory ArrayDescriptorFactory { get; private set; }
        public IIndexContainerFactory IndexContainerFactory { get; private set; }
        public IIndexDefinitionFactory IndexDefinitionFactory { get; private set; }
        public IMemoryAliasFactory MemoryAliasFactory { get; private set; }
        public IStackContextFactory StackContextFactory { get; private set; }
        public IObjectDescriptorFactory ObjectDescriptorFactory { get; private set; }
        public IObjectValueContainerFactory ObjectValueContainerFactory { get; private set; }

        public StructuralContainersFactories(
            IArrayDescriptorFactory arrayDescriptorFactory,
            IIndexContainerFactory indexContainerFactory,
            IIndexDefinitionFactory indexDefinitionFactory,
            IMemoryAliasFactory memoryAliasFactory,
            IStackContextFactory stackContextFactory,
            IObjectDescriptorFactory objectDescriptorFactory,
            IObjectValueContainerFactory objectValueContainerFactory
            )
        {
            this.ArrayDescriptorFactory = arrayDescriptorFactory;
            this.IndexContainerFactory = indexContainerFactory;
            this.IndexDefinitionFactory = indexDefinitionFactory;
            this.MemoryAliasFactory = memoryAliasFactory;
            this.StackContextFactory = stackContextFactory;
            this.ObjectDescriptorFactory = objectDescriptorFactory;
            this.ObjectValueContainerFactory = objectValueContainerFactory;
        }
    }
}
