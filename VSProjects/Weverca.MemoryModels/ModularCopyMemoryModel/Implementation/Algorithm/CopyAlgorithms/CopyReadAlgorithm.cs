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
using PHP.Core;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.ValueVisitors;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms
{
    class CopyReadAlgorithmFactory : IAlgorithmFactory<IReadAlgorithm>
    {
        public IReadAlgorithm CreateInstance(ModularMemoryModelFactories factories)
        {
            return new CopyReadAlgorithm(factories);
        }
    }

    /// <summary>
    /// Universal implementation of read algorithm. Can be used in all implementation variants.
    /// </summary>
    class CopyReadAlgorithm : AlgorithmBase, IReadAlgorithm
    {
        public CopyReadAlgorithm(ModularMemoryModelFactories factories)
            : base(factories)
        {

        }

        /// <inheritdoc />
        public MemoryEntry Read(Snapshot snapshot, MemoryPath path)
        {
            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            ReadWorker worker = new ReadWorker(snapshot);
            return worker.ReadValue(collector);
        }

        /// <inheritdoc />
        public bool IsDefined(Snapshot snapshot, MemoryPath path)
        {
            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            return collector.IsDefined;
        }

        /// <inheritdoc />
        public IEnumerable<VariableIdentifier> GetFields(Snapshot snapshot, MemoryEntry values)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.CollectFields(snapshot);
        }

        /// <inheritdoc />
        public IEnumerable<MemberIdentifier> GetIndexes(Snapshot snapshot, MemoryEntry values)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.CollectIndexes(snapshot);
        }

        /// <inheritdoc />
        public IEnumerable<FunctionValue> GetMethod(Snapshot snapshot, MemoryEntry values, QualifiedName methodName)
        {
            return snapshot.resolveMethod(values, methodName);
        }

        /// <inheritdoc />
        public IEnumerable<TypeValue> GetObjectType(Snapshot snapshot, MemoryEntry values)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.ResolveObjectsTypes(snapshot);
        }
    }
}