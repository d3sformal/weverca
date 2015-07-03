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
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyMergeAlgorithm : IMergeAlgorithm, IAlgorithmFactory<IMergeAlgorithm>
    {
        private ISnapshotStructureProxy structure;
        private ISnapshotDataProxy data;
        private int localLevel;

        /// <inheritdoc />
        public IMergeAlgorithm CreateInstance()
        {
            return new CopyMergeAlgorithm();
        }

        /// <inheritdoc />
        public void Extend(Snapshot extendedSnapshot, Snapshot sourceSnapshot)
        {
            switch (extendedSnapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    structure = extendedSnapshot.MemoryModelFactory.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
                    data = extendedSnapshot.MemoryModelFactory.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);
                    break;

                case SnapshotMode.InfoLevel:
                    data = extendedSnapshot.MemoryModelFactory.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Infos);
                    extendedSnapshot.AssignCreatedAliases(extendedSnapshot, data);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + extendedSnapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void ExtendAsCall(Snapshot extendedSnapshot, Snapshot sourceSnapshot, ProgramPointGraph calleeProgramPoint, MemoryEntry thisObject)
        {
            switch (extendedSnapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    structure = extendedSnapshot.MemoryModelFactory.SnapshotStructureFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Structure);
                    data = extendedSnapshot.MemoryModelFactory.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Data);
                    localLevel = sourceSnapshot.CallLevel + 1;

                    structure.Writeable.AddLocalLevel();
                    break;

                case SnapshotMode.InfoLevel:
                    data = extendedSnapshot.MemoryModelFactory.SnapshotDataFactory.CopyInstance(extendedSnapshot, sourceSnapshot.Infos);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + extendedSnapshot.CurrentMode);
            } 
        }

        /// <inheritdoc />
        public void Merge(Snapshot snapshot, List<Snapshot> snapshots)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        localLevel = findMaxCallLevel(snapshots);

                        MergeWorker worker = new MergeWorker(snapshot, snapshots, localLevel);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Data;
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        localLevel = findMaxCallLevel(snapshots);
                        MergeInfoWorker worker = new MergeInfoWorker(snapshot, snapshots, localLevel);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Infos;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        private int findMaxCallLevel(List<Snapshot> snapshots)
        {
            int callLevel = -1;
            foreach (var snapshot in snapshots)
            {
                if (callLevel > 0)
                {
                    if (callLevel != snapshot.CallLevel)
                    {
                        throw new NotImplementedException("Cannot merge snapshots with different call levels");
                    }
                }
                else
                {
                    callLevel = snapshot.CallLevel;
                }
            }

            return callLevel;
        }

        /// <inheritdoc />
        public void MergeAtSubprogram(Snapshot snapshot, List<Snapshot> snapshots, ProgramPointBase[] extendedPoints)
        {
            Merge(snapshot, snapshots);
        }

        /// <inheritdoc />
        public void MergeWithCall(Snapshot snapshot, Snapshot callSnapshot, List<Snapshot> snapshots)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    {
                        localLevel = callSnapshot.CallLevel;
                        MergeWorker worker = new MergeWorker(snapshot, snapshots, localLevel, true);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Data;
                    }
                    break;

                case SnapshotMode.InfoLevel:
                    {
                        localLevel = callSnapshot.CallLevel;
                        MergeInfoWorker worker = new MergeInfoWorker(snapshot, snapshots, localLevel, true);
                        worker.Merge();

                        structure = worker.Structure;
                        data = worker.Infos;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <inheritdoc />
        public void MergeMemoryEntry(Snapshot snapshot, TemporaryIndex temporaryIndex, MemoryEntry dataEntry)
        {
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);
        }

        /// <inheritdoc />
        public ISnapshotStructureProxy GetMergedStructure()
        {
            return structure;
        }

        /// <inheritdoc />
        public int GetMergedLocalLevelNumber()
        {
            return localLevel;
        }

        /// <inheritdoc />
        public ISnapshotDataProxy GetMergedData()
        {
            return data;
        }

        /*private void assignCreatedAliases(Snapshot snapshot)
        {
            foreach (IMemoryAlias aliasData in snapshot.CreatedAliases)
            {
                MemoryEntry entry = data.Readonly.GetMemoryEntry(aliasData.SourceIndex);
                foreach (MemoryIndex mustAlias in aliasData.MustAliases)
                {
                    if (mustAlias != null)
                    {
                        data.Writeable.SetMemoryEntry(mustAlias, entry);
                    }
                }

                foreach (MemoryIndex mayAlias in aliasData.MayAliases)
                {
                    if (mayAlias != null)
                    {
                        MemoryEntry aliasEntry = data.Readonly.GetMemoryEntry(mayAlias);
                        HashSet<Value> values = new HashSet<Value>(aliasEntry.PossibleValues);
                        CollectionTools.AddAll(values, entry.PossibleValues);
                        data.Writeable.SetMemoryEntry(mayAlias, snapshot.CreateMemoryEntry(values));
                    }
                }
            }
        }*/
    }
}