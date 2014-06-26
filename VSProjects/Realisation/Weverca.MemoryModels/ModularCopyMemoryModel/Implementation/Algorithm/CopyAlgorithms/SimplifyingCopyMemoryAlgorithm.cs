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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class SimplifyingCopyMemoryAlgorithm : IMemoryAlgorithm, IAlgorithmFactory<IMemoryAlgorithm>
    {
        /// <inheritdoc />
        public IMemoryAlgorithm CreateInstance()
        {
            return new CopyMemoryAlgorithm();
        }

        /// <inheritdoc />
        public void CopyMemory(Snapshot snapshot, MemoryIndex sourceIndex, MemoryIndex targetIndex, bool isMust)
        {
            CopyWithinSnapshotWorker worker = new CopyWithinSnapshotWorker(snapshot, isMust);
            worker.Copy(sourceIndex, targetIndex);
        }

        /// <inheritdoc />
        public void DestroyMemory(Snapshot snapshot, MemoryIndex index)
        {
            DestroyMemoryVisitor visitor = new DestroyMemoryVisitor(snapshot, index);

            MemoryEntry entry;
            if (snapshot.CurrentData.Readonly.TryGetMemoryEntry(index, out entry))
            {
                visitor.VisitMemoryEntry(entry);
            }
            snapshot.CurrentData.Writeable.SetMemoryEntry(index, new MemoryEntry(snapshot.UndefinedValue));
        }

        /// <inheritdoc />
        public MemoryEntry CreateMemoryEntry(Snapshot snapshot, IEnumerable<Value> values)
        {
            MemoryEntry entry = new MemoryEntry(values);
            if (entry.Count > snapshot.SimplifyLimit)
            {
                return snapshot.MemoryAssistant.Simplify(entry);
            }
            else
            {
                return new MemoryEntry(values);
            }
        }
    }
}
