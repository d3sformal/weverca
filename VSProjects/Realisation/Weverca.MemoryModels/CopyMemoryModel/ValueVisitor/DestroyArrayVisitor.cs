using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Search memory entry for associative array and releases it from given snapshot.
    /// </summary>
    class DestroyArrayVisitor : AbstractValueVisitor
    {        
        private MemoryIndex parentIndex;
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyArrayVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="parentIndex">Index of the parent.</param>
        public DestroyArrayVisitor(Snapshot snapshot, MemoryIndex parentIndex)
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
    }
}
