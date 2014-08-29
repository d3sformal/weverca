/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
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

            MemoryEntry entry = snapshot.Structure.GetMemoryEntry(sourceIndex);

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
                MemoryEntry oldEntry = snapshot.Structure.GetMemoryEntry(location.ContainingIndex);
                HashSet<Value> newValues = new HashSet<Value>();
                HashSetTools.AddAll(newValues, oldEntry.PossibleValues);

                if (isMust)
                {
                    newValues.Remove(location.Value);
                }

                IEnumerable<Value> values = location.WriteValues(snapshot.MemoryAssistant, entry);
                HashSetTools.AddAll(newValues, values);

                snapshot.Structure.SetMemoryEntry(location.ContainingIndex, new MemoryEntry(newValues));
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
                MemoryEntry oldEntry = snapshot.Structure.GetMemoryEntry(location.ContainingIndex);
                
                HashSet<Value> newValues = new HashSet<Value>();
                HashSetTools.AddAll(newValues, oldEntry.PossibleValues);

                IEnumerable<Value> values = location.WriteValues(snapshot.MemoryAssistant, entry);
                newValues.Add(location.Value);

                snapshot.Structure.SetMemoryEntry(location.ContainingIndex, new MemoryEntry(newValues));
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