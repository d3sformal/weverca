using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm
{
    public class AlgorithmInstances
    {
        public IAssignAlgorithm AssignAlgorithm { get; private set; }
        public IReadAlgorithm ReadAlgorithm { get; private set; }
        public ICommitAlgorithm CommitAlgorithm { get; private set; }
        public IMemoryAlgorithm MemoryAlgorithm { get; private set; }
        public IMergeAlgorithm MergeAlgorithm { get; private set; }
        public IPrintAlgorithm PrintAlgorithm { get; private set; }


        public AlgorithmInstances(
            IAssignAlgorithm assignAlgorithm,
            IReadAlgorithm readAlgorithm,
            ICommitAlgorithm commitAlgorithm,
            IMemoryAlgorithm memoryAlgorithm,
            IMergeAlgorithm mergeAlgorithm,
            IPrintAlgorithm printAlgorithm
            )
        {
            this.AssignAlgorithm = assignAlgorithm;
            this.ReadAlgorithm = readAlgorithm;
            this.CommitAlgorithm = commitAlgorithm;
            this.MemoryAlgorithm = memoryAlgorithm;
            this.MergeAlgorithm = mergeAlgorithm;
            this.PrintAlgorithm = printAlgorithm;
        }
    }
}
