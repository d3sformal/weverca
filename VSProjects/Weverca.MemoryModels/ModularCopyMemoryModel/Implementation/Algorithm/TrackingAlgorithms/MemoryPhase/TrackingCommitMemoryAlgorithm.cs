using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryPhase
{
    class TrackingCommitMemoryAlgorithmFactory : IAlgorithmFactory<ICommitAlgorithm>
    {
        public ICommitAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new TrackingCommitMemoryAlgorithm(factories);
        }
    }

    class TrackingCommitMemoryAlgorithm : AlgorithmBase, ICommitAlgorithm
    {
        public TrackingCommitMemoryAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }
        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            TrackingCommitWorker worker = new TrackingCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Data, snapshot.OldData);

            return worker.CompareStructureAndSimplify(false);
        }

        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            TrackingCommitWorker worker = new TrackingCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Data, snapshot.OldData);

            return worker.CompareStructureAndSimplify(true);
        }
    }
}
