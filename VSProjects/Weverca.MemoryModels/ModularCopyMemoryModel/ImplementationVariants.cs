using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.InfoPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.InfoPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.InfoPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms.MemoryPhase;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel
{
    public class ModularMemoryModelVariants
    {
        public static readonly ModularMemoryModelFactory CopyImplementation = new ModularMemoryModelFactoryBuilder()
        {
            SnapshotStructureFactory = new CopySnapshotStructureFactory(),

            SnapshotDataFactory = new CopySnapshotDataFactory(),

            MemoryAlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new CopyAssignMemoryAlgorithmFactory(),
                CommitAlgorithmFactory = new CopyCommitMemoryAlgorithmFactory(),
                MemoryAlgorithmFactory = new CopyMemoryAlgorithmFactory(),
                MergeAlgorithmFactory = new CopyMergeMemoryAlgorithmFactory(),
                ReadAlgorithmFactory = new CopyReadAlgorithmFactory(),
                PrintAlgorithmFactory = new PrintAlgorithmFactory()
            }.Build(),

            InfoAlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new CopyAssignInfoAlgorithmFactory(),
                CommitAlgorithmFactory = new CopyCommitInfoAlgorithmFactory(),
                MemoryAlgorithmFactory = new CopyMemoryAlgorithmFactory(),
                MergeAlgorithmFactory = new CopyMergeInfoAlgorithmFactory(),
                ReadAlgorithmFactory = new CopyReadAlgorithmFactory(),
                PrintAlgorithmFactory = new PrintAlgorithmFactory()
            }.Build()

        }.Build();

        public static readonly ModularMemoryModelFactory LazyImplementation = new ModularMemoryModelFactoryBuilder()
        {
            SnapshotStructureFactory = new LazyCopySnapshotStructureFactory(),

            SnapshotDataFactory = new LazyCopySnapshotDataFactory(),

            MemoryAlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new CopyAssignMemoryAlgorithmFactory(),
                CommitAlgorithmFactory = new LazyCommitMemoryAlgorithmFactory(),
                MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithmFactory(),
                MergeAlgorithmFactory = new CopyMergeMemoryAlgorithmFactory(),
                ReadAlgorithmFactory = new CopyReadAlgorithmFactory(),
                PrintAlgorithmFactory = new PrintAlgorithmFactory()
            }.Build(),

            InfoAlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new CopyAssignInfoAlgorithmFactory(),
                CommitAlgorithmFactory = new LazyCommitInfoAlgorithmFactory(),
                MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithmFactory(),
                MergeAlgorithmFactory = new CopyMergeInfoAlgorithmFactory(),
                ReadAlgorithmFactory = new CopyReadAlgorithmFactory(),
                PrintAlgorithmFactory = new PrintAlgorithmFactory()
            }.Build()

        }.Build();

        public static readonly ModularMemoryModelFactory TrackingImplementation = new ModularMemoryModelFactoryBuilder()
        {
            SnapshotStructureFactory = new TrackingSnapshotStructureFactory(),

            SnapshotDataFactory = new TrackingSnapshotDataFactory(),

            MemoryAlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new LazyAssignMemoryAlgorithmFactory(),
                CommitAlgorithmFactory = new TrackingCommitMemoryAlgorithmFactory(),
                MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithmFactory(),
                MergeAlgorithmFactory = new TrackingMergeMemoryAlgorithmFactory(),
                ReadAlgorithmFactory = new CopyReadAlgorithmFactory(),
                PrintAlgorithmFactory = new PrintAlgorithmFactory()
            }.Build(),

            InfoAlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new LazyAssignInfoAlgorithmFactory(),
                CommitAlgorithmFactory = new TrackingCommitInfoAlgorithmFactory(),
                MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithmFactory(),
                MergeAlgorithmFactory = new TrackingMergeInfoAlgorithmFactory(),
                ReadAlgorithmFactory = new CopyReadAlgorithmFactory(),
                PrintAlgorithmFactory = new PrintAlgorithmFactory()
            }.Build()

        }.Build();
    }
}
