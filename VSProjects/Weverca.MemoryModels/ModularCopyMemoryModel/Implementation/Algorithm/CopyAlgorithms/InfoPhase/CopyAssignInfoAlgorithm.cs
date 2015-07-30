using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.InfoPhase
{
    class CopyAssignInfoAlgorithmFactory : IAlgorithmFactory<IAssignAlgorithm>
    {
        public IAssignAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyAssignInfoAlgorithm(factories);
        }
    }

    /// <summary>
    /// Copy implementation of assign algorithm for assign in info phase. 
    /// Writes given data to an existing locations.
    /// </summary>
    class CopyAssignInfoAlgorithm : AlgorithmBase, IAssignAlgorithm
    {
        public CopyAssignInfoAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        /// <inheritdoc />
        public void Assign(Snapshot snapshot, Memory.MemoryPath path, AnalysisFramework.Memory.MemoryEntry value, bool forceStrongWrite)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            if (forceStrongWrite)
            {
                collector.SetAllToMust();
            }

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(Factories, snapshot);
            worker.Assign(collector, value);
        }

        /// <inheritdoc />
        public void AssignAlias(Snapshot snapshot, Memory.MemoryPath targetPath, Memory.MemoryPath sourcePath)
        {
            // Do nothing - Alias cannot be assigned in info mode
        }

        /// <inheritdoc />
        public void WriteWithoutCopy(Snapshot snapshot, Memory.MemoryPath path, AnalysisFramework.Memory.MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(Factories, snapshot);
            worker.Assign(collector, value);
        }
    }
}
