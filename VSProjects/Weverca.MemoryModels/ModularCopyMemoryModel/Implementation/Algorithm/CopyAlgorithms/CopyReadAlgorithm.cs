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
        private CopyReadAlgorithm instance = new CopyReadAlgorithm();

        public IReadAlgorithm CreateInstance()
        {
            return instance;
        }
    }


    class CopyReadAlgorithm : IReadAlgorithm
    {

        public MemoryEntry Read(Snapshot snapshot, MemoryPath path)
        {
            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            ReadWorker worker = new ReadWorker(snapshot);
            return worker.ReadValue(collector);
        }

        public bool IsDefined(Snapshot snapshot, MemoryPath path)
        {
            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            return collector.IsDefined;
        }

        public IEnumerable<VariableIdentifier> GetFields(Snapshot snapshot, MemoryEntry values)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.CollectFields(snapshot);
        }

        public IEnumerable<MemberIdentifier> GetIndexes(Snapshot snapshot, MemoryEntry values)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.CollectIndexes(snapshot);
        }

        public IEnumerable<FunctionValue> GetMethod(Snapshot snapshot, MemoryEntry values, QualifiedName methodName)
        {
            return snapshot.resolveMethod(values, methodName);
        }

        public IEnumerable<TypeValue> GetObjectType(Snapshot snapshot, MemoryEntry values)
        {
            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(values);

            return visitor.ResolveObjectsTypes(snapshot);
        }
    }
}