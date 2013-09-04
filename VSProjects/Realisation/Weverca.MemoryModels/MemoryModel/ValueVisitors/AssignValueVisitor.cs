using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis.Memory;

namespace Weverca.MemoryModels.MemoryModel.ValueVisitors
{
    /// <summary>
    /// Copies given selected entry
    /// For array and object values is called snapsot method which provides copy of structure values
    /// </summary>
    class AssignValueVisitor : AbstractValueVisitor
    {
        private Snapshot snapshot;
        private MemoryIndex index;
        private List<Value> values = new List<Value>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignValueVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="index">The index.</param>
        public AssignValueVisitor(Snapshot snapshot, MemoryIndex index)
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
            values.Add(value);
        }

        /// <summary>
        /// Visits the associative array.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            AssociativeArray arrayValue = snapshot.AssignArrayValue(index, value);
            values.Add(arrayValue);
        }

        /// <summary>
        /// Visits the object value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void VisitObjectValue(ObjectValue value)
        {
            ObjectValue objectValue = snapshot.AssignObjectValue(index, value);
            values.Add(objectValue);
        }

        /// <summary>
        /// Gets the copied entry.
        /// </summary>
        /// <returns></returns>
        internal MemoryEntry GetCopiedEntry()
        {
            return new MemoryEntry(values);
        }
    }
}
