using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    /// <summary>
    /// Destroys given selected entry
    /// For array and object values is called snapsot method which provides delete of structure values
    /// </summary>
    class DestroyemoryEntryVisitor : AbstractValueVisitor
    {
        private Snapshot snapshot;
        private MemoryIndex index;

        /// <summary>
        /// Initializes a new instance of the <see cref="DestroyemoryEntryVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="index">The index.</param>
        public DestroyemoryEntryVisitor(Snapshot snapshot, MemoryIndex index)
        {
            this.snapshot = snapshot;
            this.index = index;
        }

        /// <summary>
        /// Visits the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitValue(Value value)
        {
        }

        /// <summary>
        /// Visits the associative array.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            snapshot.DestroyArrayValue(index, value);
        }

        /// <summary>
        /// Visits the object value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitObjectValue(ObjectValue value)
        {
            snapshot.DestroyObjectValue(index, value);
        }
    }
}
