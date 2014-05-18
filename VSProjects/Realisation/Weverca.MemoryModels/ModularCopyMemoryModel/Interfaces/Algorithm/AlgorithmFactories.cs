using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    /// <summary>
    /// Contains factories for creating all algorithms for memory model.
    /// 
    /// Imutable class. For creating new instances use the builder class AlgorithmFactoriesBuilder. 
    /// </summary>
    public class AlgorithmFactories
    {
        /// <summary>
        /// Gets the assign algorithm factory.
        /// </summary>
        /// <value>
        /// The assign algorithm factory.
        /// </value>
        public IAlgorithmFactory<IAssignAlgorithm> AssignAlgorithmFactory { get; private set; }
        
        /// <summary>
        /// Gets the read algorithm factory.
        /// </summary>
        /// <value>
        /// The read algorithm factory.
        /// </value>
        public IAlgorithmFactory<IReadAlgorithm> ReadAlgorithmFactory { get; private set; }
        
        /// <summary>
        /// Gets the commit algorithm factory.
        /// </summary>
        /// <value>
        /// The commit algorithm factory.
        /// </value>
        public IAlgorithmFactory<ICommitAlgorithm> CommitAlgorithmFactory { get; private set; }

        /// <summary>
        /// Gets the memory algorithm factory.
        /// </summary>
        /// <value>
        /// The memory algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMemoryAlgorithm> MemoryAlgorithmFactory { get; private set; }
        
        /// <summary>
        /// Gets the merge algorithm factory.
        /// </summary>
        /// <value>
        /// The merge algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMergeAlgorithm> MergeAlgorithmFactory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmFactories"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public AlgorithmFactories(AlgorithmFactoriesBuilder builder)
        {
            this.AssignAlgorithmFactory = builder.AssignAlgorithmFactory;
            this.ReadAlgorithmFactory = builder.ReadAlgorithmFactory;
            this.CommitAlgorithmFactory = builder.CommitAlgorithmFactory;
            this.MemoryAlgorithmFactory = builder.MemoryAlgorithmFactory;
            this.MergeAlgorithmFactory = builder.MergeAlgorithmFactory;
        }
    }


    /// <summary>
    /// Mutable version of AlgorithmFactories class.
    /// 
    /// Allows programmer to set factories within this builder class.
    /// </summary>
    public class AlgorithmFactoriesBuilder 
    {
        /// <summary>
        /// Gets or sets the assign algorithm factory.
        /// </summary>
        /// <value>
        /// The assign algorithm factory.
        /// </value>
        public IAlgorithmFactory<IAssignAlgorithm> AssignAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the read algorithm factory.
        /// </summary>
        /// <value>
        /// The read algorithm factory.
        /// </value>
        public IAlgorithmFactory<IReadAlgorithm> ReadAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the commit algorithm factory.
        /// </summary>
        /// <value>
        /// The commit algorithm factory.
        /// </value>
        public IAlgorithmFactory<ICommitAlgorithm> CommitAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the memory algorithm factory.
        /// </summary>
        /// <value>
        /// The memory algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMemoryAlgorithm> MemoryAlgorithmFactory { get; set; }

        /// <summary>
        /// Gets or sets the merge algorithm factory.
        /// </summary>
        /// <value>
        /// The merge algorithm factory.
        /// </value>
        public IAlgorithmFactory<IMergeAlgorithm> MergeAlgorithmFactory { get; set; }

        /// <summary>
        /// Creates new AlgorithmFactories collection.
        /// </summary>
        /// <returns>New AlgorithmFactories collection.</returns>
        public AlgorithmFactories Build()
        {
            return new AlgorithmFactories(this);
        }
    }
}
