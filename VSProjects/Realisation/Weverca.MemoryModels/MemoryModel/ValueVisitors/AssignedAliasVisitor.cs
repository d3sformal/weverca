using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    /// <summary>
    /// For object and array values calls method which adds new member of alias structure to the descriptor of value
    /// </summary>
    class AssignedAliasVisitor : AbstractValueVisitor
    {
        private Snapshot snapshot;
        private MemoryIndex index;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignedAliasVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="index">The index.</param>
        public AssignedAliasVisitor(Snapshot snapshot, MemoryIndex index)
        {
            this.snapshot = snapshot;
            this.index = index;
        }

        public override void VisitValue(Value value)
        {
        }

        /// <summary>
        /// Visits the associative array.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            snapshot.AliasAssignedArrayValue(index, value);
        }

        /// <summary>
        /// Visits the object value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitObjectValue(ObjectValue value)
        {
            snapshot.AliasAssignedObjectValue(index, value);
        }
    }
}
