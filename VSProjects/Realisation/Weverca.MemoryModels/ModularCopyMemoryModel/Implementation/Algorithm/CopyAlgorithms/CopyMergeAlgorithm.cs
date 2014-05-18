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
    class CopyMergeAlgorithm : IMergeAlgorithm, IAlgorithmFactory<IMergeAlgorithm>
    {
        /// <inheritdoc />
        public IMergeAlgorithm CreateInstance()
        {
            return new CopyMergeAlgorithm();
        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, List<Snapshot> snapshots)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy GetMergedStructure()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISnapshotDataProxy GetMergedData()
        {
            throw new NotImplementedException();
        }
    }
}
