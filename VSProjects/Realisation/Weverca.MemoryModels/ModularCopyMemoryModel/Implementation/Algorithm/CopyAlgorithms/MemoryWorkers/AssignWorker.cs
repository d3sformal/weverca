using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.IndexCollectors;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Tools;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Algorithm.CopyAlgorithms.MemoryWorkers
{
    /// <summary>
    /// Provides assign operation. Writes data from given temporary locations into all locations
    /// specified by given collector in may and must indexes.
    /// 
    /// Structure of array trees are deply copied. Data of must indexes are erased first. May
    /// locations are weakly updated - structure and data are merge with the existing.
    /// </summary>
    class AssignWorker
    {
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignWorker"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AssignWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Assigns the specified collector.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <param name="sourceIndex">Index of the source.</param>
        internal void Assign(IIndexCollector collector, MemoryIndex sourceIndex)
        {
            foreach (MemoryIndex mustIndex in collector.MustIndexes)
            {
                snapshot.DestroyMemory(mustIndex);
                CopyWithinSnapshotWorker copyWorker = new CopyWithinSnapshotWorker(snapshot, true);
                copyWorker.Copy(sourceIndex, mustIndex);
            }

            foreach (MemoryIndex mayIndex in collector.MayIndexes)
            {
                MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
                mergeWorker.MergeIndexes(mayIndex, sourceIndex);
            }

            MemoryEntry entry = snapshot.CurrentData.Readonly.GetMemoryEntry(sourceIndex);

            LocationVisitor mustVisitor = new LocationVisitor(snapshot, entry, true);
            foreach (ValueLocation location in collector.MustLocation)
            {
                location.Accept(mustVisitor);
            }

            LocationVisitor mayVisitor = new LocationVisitor(snapshot, entry, false);
            foreach (ValueLocation location in collector.MayLocaton)
            {
                location.Accept(mayVisitor);
            }
        }

        /// <summary>
        /// Implementation of value location visitor to process write operation on non array and
        /// object values using indexes and fields.
        /// 
        /// Visitor ignore most of
        /// </summary>
        class LocationVisitor : IValueLocationVisitor
        {
            Snapshot snapshot;
            MemoryEntry entry;
            bool isMust;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocationVisitor"/> class.
            /// </summary>
            /// <param name="snapshot">The snapshot.</param>
            /// <param name="entry">The entry.</param>
            /// <param name="isMust">if set to <c>true</c> [is must].</param>
            public LocationVisitor(Snapshot snapshot, MemoryEntry entry, bool isMust)
            {
                this.snapshot = snapshot;
                this.entry = entry;
                this.isMust = isMust;
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

                if (isMust)
                {
                    newValues.Remove(location.Value);
                }

                IEnumerable<Value> values = location.WriteValues(snapshot.MemoryAssistant, entry);
                CollectionTools.AddAll(newValues, values);

                snapshot.CurrentData.Writeable.SetMemoryEntry(location.ContainingIndex, new MemoryEntry(newValues));
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

                snapshot.CurrentData.Writeable.SetMemoryEntry(location.ContainingIndex, new MemoryEntry(newValues));
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
}
