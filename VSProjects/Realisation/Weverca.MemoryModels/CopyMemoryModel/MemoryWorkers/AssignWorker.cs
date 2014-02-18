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
    class AssignWorker
    {
        private Snapshot snapshot;

        public AssignWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

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
            foreach (CollectedLocation location in collector.MustLocation)
            {
                location.Accept(mustVisitor);
            }

            LocationVisitor mayVisitor = new LocationVisitor(snapshot, entry, false);
            foreach (CollectedLocation location in collector.MayLocaton)
            {
                location.Accept(mayVisitor);
            }
        }

        class LocationVisitor : ICollectedLocationVisitor
        {
            Snapshot snapshot;
            MemoryEntry entry;
            bool isMust;
            public LocationVisitor(Snapshot snapshot, MemoryEntry entry, bool isMust)
            {
                this.snapshot = snapshot;
                this.entry = entry;
                this.isMust = isMust;
            }

            public void VisitObjectValueLocation(ObjectValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }

            public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }

            public void VisitArrayValueLocation(ArrayValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }

            public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }

            public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
            {
                MemoryEntry oldEntry = snapshot.Structure.GetMemoryEntry(location.ContainingIndex);
                HashSet<Value> newValues = new HashSet<Value>();
                HashSetTools.AddAll(newValues, oldEntry.PossibleValues);

                if (isMust)
                {
                    newValues.Remove(location.Value);
                }

                IEnumerable<Value> values = location.WriteValues(snapshot.Assistant, entry);
                HashSetTools.AddAll(newValues, values);

                snapshot.Structure.SetMemoryEntry(location.ContainingIndex, new MemoryEntry(newValues));
            }

            public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }

            public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }


            public void VisitInfoValueLocation(InfoValueLocation location)
            {
                MemoryEntry oldEntry = snapshot.Structure.GetMemoryEntry(location.ContainingIndex);
                
                HashSet<Value> newValues = new HashSet<Value>();
                HashSetTools.AddAll(newValues, oldEntry.PossibleValues);

                IEnumerable<Value> values = location.WriteValues(snapshot.Assistant, entry);
                newValues.Add(location.Value);

                snapshot.Structure.SetMemoryEntry(location.ContainingIndex, new MemoryEntry(newValues));
            }


            public void VisitAnyStringValueLocation(AnyStringValueLocation location)
            {
                location.WriteValues(snapshot.Assistant, entry);
            }
        }
    }
}
