using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Searches memory entry for associative array and object values. Found values are removed from the given snapshot.
    /// </summary>
    class DestroyMemoryVisitor : AbstractValueVisitor
    {        
        private MemoryIndex parentIndex;
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyMemoryVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="parentIndex">Index of the parent.</param>
        public DestroyMemoryVisitor(Snapshot snapshot, MemoryIndex parentIndex)
        {
            this.snapshot = snapshot;
            this.parentIndex = parentIndex;
        }

        public override void VisitValue(Value value)
        {
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            snapshot.DestroyArray(parentIndex);
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            snapshot.DestroyObject(parentIndex, value);
        }
    }
}
