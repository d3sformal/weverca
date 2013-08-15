using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    /// <summary>
    /// 
    /// </summary>
    class AssignValueVisitor : AbstractValueVisitor
    {
        private Snapshot snapshot;
        private MemoryIndex index;
        private List<Value> values = new List<Value>();

        public AssignValueVisitor(Snapshot snapshot, MemoryIndex index)
        {
            this.snapshot = snapshot;
            this.index = index;
        }

        public override void VisitValue(Value value)
        {
            values.Add(value);
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            AssociativeArray arrayValue = snapshot.AssignArrayValue(index, value);
            values.Add(arrayValue);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            ObjectValue objectValue = snapshot.AssignObjectValue(index, value);
            values.Add(objectValue);
        }

        internal MemoryEntry GetCopiedEntry()
        {
            return new MemoryEntry(values);
        }
    }
}
