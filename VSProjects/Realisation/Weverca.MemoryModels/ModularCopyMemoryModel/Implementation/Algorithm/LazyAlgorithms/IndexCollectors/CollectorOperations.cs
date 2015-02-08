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
/*    class CollectorOperations
    {
        CollectorNode node;

        private List<KeyValuePair<string, CollectorNode>> newChildren = new List<KeyValuePair<string, CollectorNode>>();

        public CollectorOperations(CollectorNode node)
        {
            this.node = node;
        }

        internal void NewImplicitChildFromAny(string name, CollectorNode node)
        {
            HasNewChildren = true;
            newChildren.Add(new KeyValuePair<string, CollectorNode>(name, node));
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
            IsCollected = true;
        }

        public bool HasNewNodes { get; set; }

        public bool HasNewChildren { get; set; }

        public IEnumerable<KeyValuePair<string, CollectorNode>> NewChildren { get { return newChildren; } }

        internal void NewCreateNodeOperation(MemoryIndex index)
        {
            MemoryIndexCreated = true;
            node.MemoryIndex = index;
        }

        public bool IsMemoryIndexNotDefined { get; set; }

        public MemoryIndex TargetIndex { get; set; }

        public bool IsCollected { get; set; }

        public bool HasNewImplicitArray { get; set; }

        public bool MemoryIndexCreated { get; set; }
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
        public AssociativeArray ArrayValue;
        public IndexOperations(CollectorNode node)
            : base(node)
        {
        }

        internal void NewImplicitArray()
        {
            HasNewImplicitArray = true;
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
    }*/
}
