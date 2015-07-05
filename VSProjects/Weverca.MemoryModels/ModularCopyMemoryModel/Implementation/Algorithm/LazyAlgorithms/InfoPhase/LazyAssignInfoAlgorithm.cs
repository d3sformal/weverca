using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.InfoPhase
{
    class LazyAssignInfoAlgorithmFactory : IAlgorithmFactory<IAssignAlgorithm>
    {
        public IAssignAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new LazyAssignInfoAlgorithm(factories);
        }
    }

    class LazyAssignInfoAlgorithm : AlgorithmBase, IAssignAlgorithm
    {
        public LazyAssignInfoAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }
        public void Assign(Snapshot snapshot, MemoryPath path, MemoryEntry value, bool forceStrongWrite)
        {
            if (snapshot.AssignInfo == null)
            {
                snapshot.AssignInfo = new AssignInfo();
            }
            MemoryIndexModificationList pathModifications = snapshot.AssignInfo.GetOrCreatePathModification(path);

            List<Tuple<MemoryIndex, HashSet<Value>>> valuesToAssign = new List<Tuple<MemoryIndex, HashSet<Value>>>();

            foreach (var item in pathModifications.Modifications)
            {
                MemoryIndex index = item.Key;
                MemoryIndexModification indexModification = item.Value;

                HashSet<Value> values = new HashSet<Value>();
                valuesToAssign.Add(new Tuple<MemoryIndex, HashSet<Value>>(index, values));

                if (indexModification.IsCollectedIndex)
                {
                    CollectionMemoryUtils.AddAll(values, value.PossibleValues);
                }

                foreach (var datasource in indexModification.Datasources)
                {
                    MemoryEntry entry;
                    if (datasource.SourceSnapshot.Infos.Readonly.TryGetMemoryEntry(datasource.SourceIndex, out entry))
                    {
                        CollectionMemoryUtils.AddAll(values, entry.PossibleValues);
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

        public void AssignAlias(Snapshot snapshot, MemoryPath targetPath, MemoryPath sourcePath)
        {
            // Do nothing - Alias cannot be assigned in info mode
        }

        public void WriteWithoutCopy(Snapshot snapshot, MemoryPath path, MemoryEntry value)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker worker
                = new Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers.AssignWithoutCopyWorker(Factories, snapshot);
            worker.Assign(collector, value);
        }
    }
}
