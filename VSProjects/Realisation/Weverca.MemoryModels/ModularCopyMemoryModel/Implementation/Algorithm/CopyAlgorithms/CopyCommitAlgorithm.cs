using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyCommitAlgorithm : ICommitAlgorithm, IAlgorithmFactory<ICommitAlgorithm>
    {
        /// <inheritdoc />
        public ICommitAlgorithm CreateInstance()
        {
            return new CopyCommitAlgorithm();
        }

        /// <inheritdoc />
        public void SetStructure(ISnapshotStructureProxy newStructure, ISnapshotStructureProxy oldStructure)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetData(ISnapshotDataProxy newData, ISnapshotDataProxy oldData)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CommitAndSimplify(Snapshot snapshot, int simplifyLimit)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CommitAndWiden(Snapshot snapshot, int simplifyLimit)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsDifferent()
        {
            throw new NotImplementedException();
        }
    }
}
