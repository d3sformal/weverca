using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    /// <summary>
    /// Removes given selected entry
    /// For array and object values is called snapsot methods which removes structure values
    /// </summary>
    class RemoveAliasVisitor : AbstractValueVisitor
    {
        private Snapshot snapshot;
        private MemoryIndex index;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveAliasVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="index">The index.</param>
        public RemoveAliasVisitor(Snapshot snapshot, MemoryIndex index)
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
            snapshot.AliasRemovedArrayValue(index, value);
        }

        /// <summary>
        /// Visits the object value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitObjectValue(ObjectValue value)
        {
            snapshot.AliasRemovedObjectValue(index, value);
        }
    }
}
