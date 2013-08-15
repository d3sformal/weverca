using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    class DestroyemoryEntryVisitor : AbstractValueVisitor
    {
        private Snapshot snapshot;
        private MemoryIndex index;

        public DestroyemoryEntryVisitor(Snapshot snapshot, MemoryIndex index)
        {
            this.snapshot = snapshot;
            this.index = index;
        }

        public override void VisitValue(Value value)
        {
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            throw new NotImplementedException();
        }
    }
}
