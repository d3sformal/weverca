using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.InfoPhase
{
    class CopyCommitInfoAlgorithmFactory : IAlgorithmFactory<ICommitAlgorithm>
    {
        public ICommitAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyCommitInfoAlgorithm(factories);
        }
    }

    class CopyCommitInfoAlgorithm : AlgorithmBase, ICommitAlgorithm
    {
        public CopyCommitInfoAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            CopyCommitWorker worker = new CopyCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(false);
        }

        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            CopyCommitWorker worker = new CopyCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(true);
        }
    }
}
