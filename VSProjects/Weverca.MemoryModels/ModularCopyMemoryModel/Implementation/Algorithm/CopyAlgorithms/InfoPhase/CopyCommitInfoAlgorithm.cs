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
        CopyCommitInfoAlgorithm instance = new CopyCommitInfoAlgorithm();

        public ICommitAlgorithm CreateInstance()
        {
            return instance;
        }
    }

    class CopyCommitInfoAlgorithm : ICommitAlgorithm
    {
        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            CopyCommitWorker worker = new CopyCommitWorker(
                snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(false);
        }

        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            CopyCommitWorker worker = new CopyCommitWorker(
                snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(true);
        }
    }
}
