using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryPhase
{
    class CopyCommitMemoryAlgorithmFactory : IAlgorithmFactory<ICommitAlgorithm>
    {
        public ICommitAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyCommitMemoryAlgorithm(factories);
        }
    }

    class CopyCommitMemoryAlgorithm : AlgorithmBase, ICommitAlgorithm
    {
        public CopyCommitMemoryAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            CopyCommitWorker worker = new CopyCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Data, snapshot.OldData);

            return worker.CompareStructureAndSimplify(false);
        }

        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            CopyCommitWorker worker = new CopyCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Data, snapshot.OldData);

            return worker.CompareStructureAndSimplify(true);
        }
    }
}
