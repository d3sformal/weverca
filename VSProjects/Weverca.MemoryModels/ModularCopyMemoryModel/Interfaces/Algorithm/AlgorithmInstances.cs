using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    /// <summary>
    /// Holds instances of all memory model algorithms. Is used in ModularMemoryModelFactories 
    /// as the colection of algorithm singletons.
    /// </summary>
    public class AlgorithmInstances
    {
        /// <summary>
        /// Gets the assign algorithm.
        /// </summary>
        /// <value>
        /// The assign algorithm.
        /// </value>
        public IAssignAlgorithm AssignAlgorithm { get; private set; }

        /// <summary>
        /// Gets the read algorithm.
        /// </summary>
        /// <value>
        /// The read algorithm.
        /// </value>
        public IReadAlgorithm ReadAlgorithm { get; private set; }

        /// <summary>
        /// Gets the commit algorithm.
        /// </summary>
        /// <value>
        /// The commit algorithm.
        /// </value>
        public ICommitAlgorithm CommitAlgorithm { get; private set; }

        /// <summary>
        /// Gets the memory algorithm.
        /// </summary>
        /// <value>
        /// The memory algorithm.
        /// </value>
        public IMemoryAlgorithm MemoryAlgorithm { get; private set; }

        /// <summary>
        /// Gets the merge algorithm.
        /// </summary>
        /// <value>
        /// The merge algorithm.
        /// </value>
        public IMergeAlgorithm MergeAlgorithm { get; private set; }

        /// <summary>
        /// Gets the print algorithm.
        /// </summary>
        /// <value>
        /// The print algorithm.
        /// </value>
        public IPrintAlgorithm PrintAlgorithm { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmInstances"/> class.
        /// </summary>
        /// <param name="assignAlgorithm">The assign algorithm.</param>
        /// <param name="readAlgorithm">The read algorithm.</param>
        /// <param name="commitAlgorithm">The commit algorithm.</param>
        /// <param name="memoryAlgorithm">The memory algorithm.</param>
        /// <param name="mergeAlgorithm">The merge algorithm.</param>
        /// <param name="printAlgorithm">The print algorithm.</param>
        public AlgorithmInstances(
            IAssignAlgorithm assignAlgorithm,
            IReadAlgorithm readAlgorithm,
            ICommitAlgorithm commitAlgorithm,
            IMemoryAlgorithm memoryAlgorithm,
            IMergeAlgorithm mergeAlgorithm,
            IPrintAlgorithm printAlgorithm
            )
        {
            this.AssignAlgorithm = assignAlgorithm;
            this.ReadAlgorithm = readAlgorithm;
            this.CommitAlgorithm = commitAlgorithm;
            this.MemoryAlgorithm = memoryAlgorithm;
            this.MergeAlgorithm = mergeAlgorithm;
            this.PrintAlgorithm = printAlgorithm;
        }
    }
}
