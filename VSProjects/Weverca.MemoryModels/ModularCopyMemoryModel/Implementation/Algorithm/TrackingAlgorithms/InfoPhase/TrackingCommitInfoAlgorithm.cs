using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.InfoPhase
{
    class TrackingCommitInfoAlgorithmFactory : IAlgorithmFactory<ICommitAlgorithm>
    {
        public ICommitAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new TrackingCommitInfoAlgorithm(factories);
        }
    }

    class TrackingCommitInfoAlgorithm : AlgorithmBase, ICommitAlgorithm
    {
        public TrackingCommitInfoAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }
        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            TrackingCommitWorker worker = new TrackingCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(false);
        }

        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            TrackingCommitWorker worker = new TrackingCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(true);
        }
    }
}
