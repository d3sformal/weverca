using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.IndexCollectors
{
    class CollectorOperations
    {
        CollectorNode node;

        public CollectorOperations(CollectorNode node)
        {
            this.node = node;
        }

        internal void NewImplicitChildFromAny(string name, MemoryIndex memoryIndex)
        {
            //throw new NotImplementedException();
        }

        internal void NewAnyNodeAccess()
        {
            //throw new NotImplementedException();
        }

        internal void NewAnyChildAccess()
        {
            //throw new NotImplementedException();
        }

        internal void NewChildAccess(string name)
        {
            //throw new NotImplementedException();
        }

        internal void NewCollectedOperation()
        {
            //throw new NotImplementedException();
        }

        public bool HasNewNodes { get; set; }

        public bool HasNewChildren { get; set; }

        public IEnumerable<KeyValuePair<string, CollectorNode>> NewChildren { get; set; }

        internal void NewCreateNodeOperation(MemoryIndex index)
        {
            throw new NotImplementedException();
        }

        public bool IsMemoryIndexNotDefined { get; set; }

        public MemoryIndex TargetIndex { get; set; }
    }

    class VariableOperations : CollectorOperations
    {
        public VariableOperations(CollectorNode node)
            : base(node)
        {
        }

    }

    class ControlOperations : CollectorOperations
    {
        public ControlOperations(CollectorNode node)
            : base(node)
        {
        }
    }

    class IndexOperations : CollectorOperations
    {
        public IndexOperations(CollectorNode node)
            : base(node)
        {
        }

        internal void NewImplicitArray()
        {
            //throw new NotImplementedException();
        }
    }

    class FieldOperations : CollectorOperations
    {
        public FieldOperations(CollectorNode node)
            : base(node)
        {
        }

        internal void NewImplicitObject()
        {
            //throw new NotImplementedException();
        }
    }
}
