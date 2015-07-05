using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.InfoPhase
{
    class LazyCommitInfoAlgorithmFactory : IAlgorithmFactory<ICommitAlgorithm>
    {
        LazyCommitInfoAlgorithm instance = new LazyCommitInfoAlgorithm();

        public ICommitAlgorithm CreateInstance()
        {
            return instance;
        }
    }

    class LazyCommitInfoAlgorithm : ICommitAlgorithm
    {
        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            LazyCommitWorker worker = new LazyCommitWorker(
                snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(false);
        }

        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            LazyCommitWorker worker = new LazyCommitWorker(
                snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(true);
        }
    }
}
