using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{    
    /// <summary>
    /// Search memory entry for object values and releases them from given snapshot.
    /// </summary>
    class DestroyObjectsVisitor : AbstractValueVisitor
    {
        private MemoryIndex parentIndex;
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyObjectsVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="parentIndex">Index of the parent.</param>
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
