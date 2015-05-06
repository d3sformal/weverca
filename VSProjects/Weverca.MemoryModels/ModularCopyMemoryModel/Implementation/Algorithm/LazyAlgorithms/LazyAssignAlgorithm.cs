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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms
{
    class LazyAssignAlgorithm : IAssignAlgorithm, IAlgorithmFactory<IAssignAlgorithm>
    {
        /// <inheritdoc />
        public IAssignAlgorithm CreateInstance()
        {
            return new LazyAssignAlgorithm();
        }

        /// <inheritdoc />
        public void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite)
        {
            if (snapshot.AssignInfo == null)
            {
                snapshot.AssignInfo = new AssignInfo();
            }
            MemoryIndexModificationList pathModifications = snapshot.AssignInfo.GetOrCreatePathModification(path);

            if (snapshot.CurrentMode == SnapshotMode.MemoryLevel)
            {
                MemoryEntryCollector entryCollector = new MemoryEntryCollector(snapshot);
                entryCollector.ProcessRootMemoryEntry(value);

                TreeIndexCollector treeCollector = new TreeIndexCollector(snapshot);
                treeCollector.PostProcessAliases = true;
                treeCollector.ProcessPath(path);

                AssignWorker worker = new AssignWorker(snapshot, entryCollector, treeCollector, pathModifications);
                worker.ForceStrongWrite = forceStrongWrite;
                worker.Assign();
            }
            else
            {
                List<Tuple<MemoryIndex, HashSet<Value>>> valuesToAssign = new List<Tuple<MemoryIndex, HashSet<Value>>>();

                foreach (var item in pathModifications.Modifications)
                {
                    MemoryIndex index = item.Key;
                    MemoryIndexModification indexModification = item.Value;

                    HashSet<Value> values = new HashSet<Value>();
                    valuesToAssign.Add(new Tuple<MemoryIndex, HashSet<Value>>(index, values));

                    if (indexModification.IsCollectedIndex)
                    {
                        CollectionTools.AddAll(values, value.PossibleValues);
                    }

                    foreach (var datasource in indexModification.Datasources)
                    {
                        MemoryEntry entry;
                        if (datasource.SourceSnapshot.Infos.Readonly.TryGetMemoryEntry(datasource.SourceIndex, out entry))
                        {
                            CollectionTools.AddAll(values, entry.PossibleValues);
                        }
                    }
                }

                foreach (var item in valuesToAssign)
                {
                    MemoryIndex index = item.Item1;
                    HashSet<Value> values = item.Item2;

                    MemoryEntry entry = new MemoryEntry(values);
                    snapshot.Infos.Writeable.SetMemoryEntry(index, entry);
                }
            }
        }

        /// <inheritdoc />
        public void AssignAlias(Snapshot snapshot, MemoryPath targetPath, MemoryPath sourcePath)
        {
            if (snapshot.CurrentMode == SnapshotMode.InfoLevel)
            {
                return;
            }

            if (snapshot.AssignInfo == null)
            {
                snapshot.AssignInfo = new AssignInfo();
            }

            // Collects memory location of alias sources
            TreeIndexCollector aliasSourcesCollector = new TreeIndexCollector(snapshot);
            aliasSourcesCollector.ProcessPath(sourcePath);

            // Creates missing source locations and collect source data
            AliasWorker aliasWorker = new AliasWorker(snapshot, aliasSourcesCollector, snapshot.AssignInfo.AliasAssignModifications);
            aliasWorker.CollectAliases();

            // Collects target locations
            TreeIndexCollector aliasTargetCollector = new TreeIndexCollector(snapshot);
            aliasTargetCollector.ProcessPath(targetPath);

            // Creates missing target locations, create aliases and assign source data
            AssignWorker assignWorker = new AssignWorker(snapshot, aliasWorker.EntryCollector, aliasTargetCollector, snapshot.AssignInfo.AliasAssignModifications);
            assignWorker.AssignAliasesIntoCollectedIndexes = true;
            assignWorker.Assign();
        }

        /// <inheritdoc />
        public void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker worker 
                = new Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker(snapshot);
            worker.Assign(collector, value);
        }
    }
}