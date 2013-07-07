using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    class AssignValueVisitor : AbstractValueVisitor
    {
        private MemoryIndex index;
        private MemoryInfo info;

        public AssignValueVisitor(MemoryIndex index, MemoryInfo info)
        {
            // TODO: Complete member initialization
            this.index = index;
            this.info = info;
        }

        public override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            throw new NotImplementedException();
        }

        internal MemoryEntry GetCopiedEntry()
        {
            throw new NotImplementedException();
        }
    }
}
