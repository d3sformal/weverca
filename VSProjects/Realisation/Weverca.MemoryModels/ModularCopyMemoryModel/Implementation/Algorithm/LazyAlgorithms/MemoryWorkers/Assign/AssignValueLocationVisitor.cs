using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.LazyAlgorithms.MemoryWorkers.Assign
{
    /// <summary>
    /// Implementation of value location visitor to process write operation on non array and
    /// object values using indexes and fields.
    /// 
    /// Visitor ignore most of
    /// </summary>
    class AssignValueLocationVisitor : IValueLocationVisitor
    {
        Snapshot snapshot;
        MemoryEntry entry;

        public bool IsMust { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocationVisitor"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="entry">The entry.</param>
        /// <param name="isMust">if set to <c>true</c> [is must].</param>
        public AssignValueLocationVisitor(Snapshot snapshot, MemoryEntry entry, bool isMust)
        {
            this.snapshot = snapshot;
            this.entry = entry;
            this.IsMust = isMust;
        }

        /// <summary>
        /// Visits the object value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitObjectValueLocation(ObjectValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }

        /// <summary>
        /// Visits the object any value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }

        /// <summary>
        /// Visits the array value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayValueLocation(ArrayValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }

        /// <summary>
        /// Visits the array any value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }

        /// <summary>
        /// Visits the array string value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
        {
            MemoryEntry oldEntry = snapshot.CurrentData.Readonly.GetMemoryEntry(location.ContainingIndex);
            HashSet<Value> newValues = new HashSet<Value>();
            CollectionTools.AddAll(newValues, oldEntry.PossibleValues);

            if (IsMust)
            {
                newValues.Remove(location.Value);
            }

            IEnumerable<Value> values = location.WriteValues(snapshot.MemoryAssistant, entry);
            CollectionTools.AddAll(newValues, values);

            snapshot.CurrentData.Writeable.SetMemoryEntry(location.ContainingIndex, snapshot.CreateMemoryEntry(newValues));
        }

        /// <summary>
        /// Visits the array undefined value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }

        /// <summary>
        /// Visits the object undefined value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }


        /// <summary>
        /// Visits the information value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitInfoValueLocation(InfoValueLocation location)
        {
            MemoryEntry oldEntry = snapshot.CurrentData.Readonly.GetMemoryEntry(location.ContainingIndex);

            HashSet<Value> newValues = new HashSet<Value>();
            CollectionTools.AddAll(newValues, oldEntry.PossibleValues);

            IEnumerable<Value> values = location.WriteValues(snapshot.MemoryAssistant, entry);
            newValues.Add(location.Value);

            snapshot.CurrentData.Writeable.SetMemoryEntry(location.ContainingIndex, snapshot.CreateMemoryEntry(newValues));
        }


        /// <summary>
        /// Visits any string value location.
        /// </summary>
        /// <param name="location">The location.</param>
        public void VisitAnyStringValueLocation(AnyStringValueLocation location)
        {
            location.WriteValues(snapshot.MemoryAssistant, entry);
        }
    }
}
