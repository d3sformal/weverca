using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyAlgorithmFactories
    {
        private static readonly AlgorithmFactories copyAlgorithmFactories = new AlgorithmFactoriesBuilder()
        {
            AssignAlgorithmFactory = new CopyAssignAlgorithm(),
            CommitAlgorithmFactory = new CopyCommitAlgorithm(),
            MemoryAlgorithmFactory = new CopyMemoryAlgorithm(),
            MergeAlgorithmFactory = new CopyMergeAlgorithm(),
            ReadAlgorithmFactory = new CopyReadAlgorithm()
        }.Build();

        public static AlgorithmFactories Factories { get { return copyAlgorithmFactories; } }
    }
}
