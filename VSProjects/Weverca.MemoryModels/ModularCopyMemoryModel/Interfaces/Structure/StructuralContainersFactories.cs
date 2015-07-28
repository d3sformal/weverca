using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Contains the set of all factories used to create instances of inner structural containers.
    /// 
    /// Instance of this class is the part of ModularMemoryModelFactories object and is transfered 
    /// to all anapshots, algrothms, structure and data instances. This instance has to be used to 
    /// create new instances of all collections instead of the NEW operator.
    /// </summary>
    public class StructuralContainersFactories
    {
        /// <summary>
        /// Gets the array descriptor factory.
        /// </summary>
        /// <value>
        /// The array descriptor factory.
        /// </value>
        public IArrayDescriptorFactory ArrayDescriptorFactory { get; private set; }

        /// <summary>
        /// Gets the index container factory.
        /// </summary>
        /// <value>
        /// The index container factory.
        /// </value>
        public IIndexContainerFactory IndexContainerFactory { get; private set; }

        /// <summary>
        /// Gets the index definition factory.
        /// </summary>
        /// <value>
        /// The index definition factory.
        /// </value>
        public IIndexDefinitionFactory IndexDefinitionFactory { get; private set; }

        /// <summary>
        /// Gets the memory alias factory.
        /// </summary>
        /// <value>
        /// The memory alias factory.
        /// </value>
        public IMemoryAliasFactory MemoryAliasFactory { get; private set; }

        /// <summary>
        /// Gets the stack context factory.
        /// </summary>
        /// <value>
        /// The stack context factory.
        /// </value>
        public IStackContextFactory StackContextFactory { get; private set; }

        /// <summary>
        /// Gets the object descriptor factory.
        /// </summary>
        /// <value>
        /// The object descriptor factory.
        /// </value>
        public IObjectDescriptorFactory ObjectDescriptorFactory { get; private set; }

        /// <summary>
        /// Gets the object value container factory.
        /// </summary>
        /// <value>
        /// The object value container factory.
        /// </value>
        public IObjectValueContainerFactory ObjectValueContainerFactory { get; private set; }

        /// <summary>
        /// Gets the declaration container factory.
        /// </summary>
        /// <value>
        /// The declaration container factory.
        /// </value>
        public IDeclarationContainerFactory DeclarationContainerFactory { get; private set; }

        /// <summary>
        /// Gets the associative container factory.
        /// </summary>
        /// <value>
        /// The associative container factory.
        /// </value>
        public IAssociativeContainerFactory AssociativeContainerFactory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuralContainersFactories"/> class.
        /// </summary>
        /// <param name="arrayDescriptorFactory">The array descriptor factory.</param>
        /// <param name="indexContainerFactory">The index container factory.</param>
        /// <param name="indexDefinitionFactory">The index definition factory.</param>
        /// <param name="memoryAliasFactory">The memory alias factory.</param>
        /// <param name="stackContextFactory">The stack context factory.</param>
        /// <param name="objectDescriptorFactory">The object descriptor factory.</param>
        /// <param name="objectValueContainerFactory">The object value container factory.</param>
        /// <param name="declarationContainerFactory">The declaration container factory.</param>
        /// <param name="associativeContainerFactory">The associative container factory.</param>
        public StructuralContainersFactories(
            IArrayDescriptorFactory arrayDescriptorFactory,
            IIndexContainerFactory indexContainerFactory,
            IIndexDefinitionFactory indexDefinitionFactory,
            IMemoryAliasFactory memoryAliasFactory,
            IStackContextFactory stackContextFactory,
            IObjectDescriptorFactory objectDescriptorFactory,
            IObjectValueContainerFactory objectValueContainerFactory,
            IDeclarationContainerFactory declarationContainerFactory,
            IAssociativeContainerFactory associativeContainerFactory
            )
        {
            this.ArrayDescriptorFactory = arrayDescriptorFactory;
            this.IndexContainerFactory = indexContainerFactory;
            this.IndexDefinitionFactory = indexDefinitionFactory;
            this.MemoryAliasFactory = memoryAliasFactory;
            this.StackContextFactory = stackContextFactory;
            this.ObjectDescriptorFactory = objectDescriptorFactory;
            this.ObjectValueContainerFactory = objectValueContainerFactory;
            this.DeclarationContainerFactory = declarationContainerFactory;
            this.AssociativeContainerFactory = associativeContainerFactory;
        }
    }
}
