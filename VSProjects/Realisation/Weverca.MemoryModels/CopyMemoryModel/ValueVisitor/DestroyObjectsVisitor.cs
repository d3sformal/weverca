using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class DestroyObjectsVisitor : AbstractValueVisitor
    {
        private MemoryIndex parentIndex;
        private Snapshot snapshot;
        public DestroyObjectsVisitor(Snapshot snapshot, MemoryIndex parentIndex)
        {
            this.snapshot = snapshot;
            this.parentIndex = parentIndex;
        }

        public override void VisitValue(Value value)
        {
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            snapshot.DestroyObject(parentIndex, value);
        }
    }
}
