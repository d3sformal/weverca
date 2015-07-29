/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    /// <summary>
    /// Generec factory interface for algorithm type.
    /// 
    /// Algorithm factory always creates new instance of algorithm. Algorithm is used only for
    /// single job. Parameters for run of algorithm will be cpecified within algorithm public
    /// interface.
    /// </summary>
    /// <typeparam name="T">Type of algorithm to create.</typeparam>
    public interface IAlgorithmFactory<T>
    {
        /// <summary>
        /// Creates new instance of algorithm.
        /// </summary>
        /// <returns>New instance of algorithm.</returns>
        T CreateInstance(ModularMemoryModelFactories factories);
    }

    /// <summary>
    /// Contains factories for creating all algorithms for memory model.
    /// 
    /// Imutable class.
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
        /// Gets the print algorithm factory.
        /// </summary>
        /// <value>
        /// The print algorithm factory.
        /// </value>
        public IAlgorithmFactory<IPrintAlgorithm> PrintAlgorithmFactory { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmFactories" /> class.
        /// </summary>
        /// <param name="assignAlgorithmFactory">The assign algorithm factory.</param>
        /// <param name="readAlgorithmFactory">The read algorithm factory.</param>
        /// <param name="commitAlgorithmFactory">The commit algorithm factory.</param>
        /// <param name="memoryAlgorithmFactory">The memory algorithm factory.</param>
        /// <param name="mergeAlgorithmFactory">The merge algorithm factory.</param>
        /// <param name="printAlgorithmFactory">The print algorithm factory.</param>
        public AlgorithmFactories(
            IAlgorithmFactory<IAssignAlgorithm> assignAlgorithmFactory,
            IAlgorithmFactory<IReadAlgorithm> readAlgorithmFactory,
            IAlgorithmFactory<ICommitAlgorithm> commitAlgorithmFactory,
            IAlgorithmFactory<IMemoryAlgorithm> memoryAlgorithmFactory,
            IAlgorithmFactory<IMergeAlgorithm> mergeAlgorithmFactory,
            IAlgorithmFactory<IPrintAlgorithm> printAlgorithmFactory
            )
        {
            this.AssignAlgorithmFactory = assignAlgorithmFactory;
            this.ReadAlgorithmFactory = readAlgorithmFactory;
            this.CommitAlgorithmFactory = commitAlgorithmFactory;
            this.MemoryAlgorithmFactory = memoryAlgorithmFactory;
            this.MergeAlgorithmFactory = mergeAlgorithmFactory;
            this.PrintAlgorithmFactory = printAlgorithmFactory;
        }

        /// <summary>
        /// Creates the instances of the algorithms using the factories stored in this container.
        /// </summary>
        /// <param name="factories">The factories object which will be sent to the instances.</param>
        /// <returns>New object with algorithm instances instances.</returns>
        public AlgorithmInstances CreateInstances(ModularMemoryModelFactories factories)
        {
            return new AlgorithmInstances(
                AssignAlgorithmFactory.CreateInstance(factories),
                ReadAlgorithmFactory.CreateInstance(factories),
                CommitAlgorithmFactory.CreateInstance(factories),
                MemoryAlgorithmFactory.CreateInstance(factories),
                MergeAlgorithmFactory.CreateInstance(factories),
                PrintAlgorithmFactory.CreateInstance(factories)
                );
        }
    }
}