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
        public ICommitAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new LazyCommitInfoAlgorithm(factories);
        }
    }

    /// <summary>
    /// Lazy implementation of commit algorithm for info phase. Provides full commit only if some part of snapshot was 
    /// modified and this is not the first transaction of the snapshot.
    /// </summary>
    class LazyCommitInfoAlgorithm : AlgorithmBase, ICommitAlgorithm
    {
        public LazyCommitInfoAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        /// <inheritdoc />
        public bool CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            LazyCommitWorker worker = new LazyCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(false);
        }

        /// <inheritdoc />
        public bool CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            LazyCommitWorker worker = new LazyCommitWorker(
                Factories, snapshot, simplifyLimit, snapshot.Structure, snapshot.OldStructure, snapshot.Infos, snapshot.OldInfos);

            return worker.CompareDataAndSimplify(true);
        }
    }
}
