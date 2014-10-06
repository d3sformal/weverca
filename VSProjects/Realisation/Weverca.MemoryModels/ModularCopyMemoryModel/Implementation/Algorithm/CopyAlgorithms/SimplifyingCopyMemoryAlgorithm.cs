/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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