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
using PHP.Core;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyReadAlgorithm : IReadAlgorithm, IAlgorithmFactory<IReadAlgorithm>
    {
        private MemoryEntry values;
        private IIndexCollector collector;
        Snapshot snapshot;

        /// <inheritdoc />
        public IReadAlgorithm CreateInstance()
        {
            return new CopyReadAlgorithm();
        }

        /// <inheritdoc />
        public void Read(Snapshot snapshot, MemoryPath path)
        {
            this.snapshot = snapshot;

            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);
            this.collector = collector;
        }

        /// <inheritdoc />
        public void Read(Snapshot snapshot, MemoryEntry values)
        {
            this.snapshot = snapshot;
            this.values = values;
        }

        /// <inheritdoc />
        public bool IsDefined()
        {
            if (values != null)
            {
                return true;
            }
            else if (collector != null)
            {
                return collector.IsDefined;
            }

            return false;
        }

        /// <inheritdoc />
        public MemoryEntry GetValue()
        {
            if (values == null)
            {
                ReadWorker worker = new ReadWorker(snapshot);
                values = worker.ReadValue(collector);
            }

            return values;
        }

        /// <inheritdoc />
        public IEnumerable<VariableIdentifier> GetFields()
        {
            MemoryEntry values = GetValue();

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.CollectFields(snapshot);
        }

        /// <inheritdoc />
        public IEnumerable<MemberIdentifier> GetIndexes()
        {
            MemoryEntry values = GetValue();

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.CollectIndexes(snapshot);
        }

        /// <inheritdoc />
        public IEnumerable<FunctionValue> GetMethod(QualifiedName methodName)
        {
            MemoryEntry values = GetValue();
            return snapshot.resolveMethod(values, methodName);
        }

        /// <inheritdoc />
        public IEnumerable<TypeValue> GetObjectType()
        {
            MemoryEntry values = GetValue();

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.ResolveObjectsTypes(snapshot);
        }
    }
}
