using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.TrackingAlgorithms;
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

            AlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new CopyAssignAlgorithm(),
                CommitAlgorithmFactory = new CopyCommitAlgorithm(),
                MemoryAlgorithmFactory = new CopyMemoryAlgorithm(),
                MergeAlgorithmFactory = new CopyMergeAlgorithm(),
                ReadAlgorithmFactory = new CopyReadAlgorithm(),
                PrintAlgorithmFactory = new PrintAlgorithm()
            }.Build()

        }.Build();

        public static readonly ModularMemoryModelFactory LazyImplementation = new ModularMemoryModelFactoryBuilder()
        {
            SnapshotStructureFactory = new LazyCopySnapshotStructureFactory(),

            SnapshotDataFactory = new LazyCopySnapshotDataFactory(),

            AlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new CopyAssignAlgorithm(),
                CommitAlgorithmFactory = new LazyCommitAlgorithm(),
                MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithm(),
                MergeAlgorithmFactory = new CopyMergeAlgorithm(),
                ReadAlgorithmFactory = new CopyReadAlgorithm(),
                PrintAlgorithmFactory = new PrintAlgorithm()
            }.Build()

        }.Build();

        public static readonly ModularMemoryModelFactory TrackingImplementation = new ModularMemoryModelFactoryBuilder()
        {
            SnapshotStructureFactory = new TrackingSnapshotStructureFactory(),

            SnapshotDataFactory = new TrackingSnapshotDataFactory(),

            AlgorithmFactories = new AlgorithmFactoriesBuilder()
            {
                AssignAlgorithmFactory = new LazyAssignAlgorithm(),
                CommitAlgorithmFactory = new TrackingCommitAlgorithm(),
                MemoryAlgorithmFactory = new SimplifyingCopyMemoryAlgorithm(),
                MergeAlgorithmFactory = new TrackingMergeAlgorithm(),
                ReadAlgorithmFactory = new CopyReadAlgorithm(),
                PrintAlgorithmFactory = new PrintAlgorithm()
            }.Build()

        }.Build();
    }
}
