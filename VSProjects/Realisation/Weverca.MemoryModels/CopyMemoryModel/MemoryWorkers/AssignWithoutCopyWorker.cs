using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    //TODO - dopsat komentare
    // zduraznit, ze prirazeni bylo udelano tak, aby neporusilo semantiku
    // prirazuje se i do vsech MUST i MAY indexu v collectoru
    // pole v cilove promenne ZUSTAVA!!!
    class AssignWithoutCopyWorker
    {
        private Snapshot snapshot;

        public AssignWithoutCopyWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        internal void Assign(AssignCollector collector, AnalysisFramework.Memory.MemoryEntry value)
        {
            CollectComposedValuesVisitor composedValues = new CollectComposedValuesVisitor();
            composedValues.VisitMemoryEntry(value);

            foreach (MemoryIndex mustIndex in collector.MustIndexes)
            {
                assignMust(mustIndex, composedValues);
            }

            foreach (MemoryIndex mayIndex in collector.MayIndexes)
            {
                assignMay(mayIndex, composedValues);
            }

            if (snapshot.CurrentMode == SnapshotMode.InfoLevel)
            {
                InfoLocationVisitor mustVisitor = new InfoLocationVisitor(snapshot, value, true);
                foreach (CollectedLocation mustLocation in collector.MustLocation)
                {
                    mustLocation.Accept(mustVisitor);
                }

                InfoLocationVisitor mayVisitor = new InfoLocationVisitor(snapshot, value, false);
                foreach (CollectedLocation mustLocation in collector.MayLocaton)
                {
                    mustLocation.Accept(mayVisitor);
                }
            }
        }

        private void assignMust(MemoryIndex mustIndex, CollectComposedValuesVisitor composedValues)
        {
            IndexData data = snapshot.Structure.GetIndexData(mustIndex);
            HashSet<Value> values = new HashSet<Value>(composedValues.Values);

            if (data.Array != null)
            {
                values.Add(data.Array);
            }

            if (composedValues.Objects.Count > 0)
            {
                snapshot.Structure.SetObjects(mustIndex, new ObjectValueContainer(composedValues.Objects));
                HashSetTools.AddAll(values, data.Objects);
            }

            snapshot.Structure.SetMemoryEntry(mustIndex, new MemoryEntry(values));
        }

        private void assignMay(MemoryIndex mayIndex, CollectComposedValuesVisitor composedValues)
        {
            IndexData data = snapshot.Structure.GetIndexData(mayIndex);
            HashSet<Value> values = new HashSet<Value>(composedValues.Values);
            
            if (composedValues.Objects.Count > 0)
            {
                HashSet<ObjectValue> objects = new HashSet<ObjectValue>(data.Objects);
                HashSetTools.AddAll(objects, composedValues.Objects);
                snapshot.Structure.SetObjects(mayIndex, new ObjectValueContainer(objects));

                HashSetTools.AddAll(values, data.Objects);
            }

            HashSetTools.AddAll(values, snapshot.Structure.GetMemoryEntry(mayIndex).PossibleValues);
            snapshot.Structure.SetMemoryEntry(mayIndex, new MemoryEntry(values));
        }

        class InfoLocationVisitor : ICollectedLocationVisitor
        {
            Snapshot snapshot;
            MemoryEntry entry;
            bool isMust;
            public InfoLocationVisitor(Snapshot snapshot, MemoryEntry entry, bool isMust)
            {
                this.snapshot = snapshot;
                this.entry = entry;
                this.isMust = isMust;
            }

            public void VisitObjectValueLocation(ObjectValueLocation location)
            {
            }

            public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            public void VisitArrayValueLocation(ArrayValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
            {
            }

            public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
            {
            }


            public void VisitInfoValueLocation(InfoValueLocation location)
            {
            }


            public void VisitAnyStringValueLocation(AnyStringValueLocation location)
            {
                assign(location.ContainingIndex);
            }

            private void assign(MemoryIndex index)
            {
                HashSet<Value> newValues = new HashSet<Value>();
                if (!isMust)
                {
                    MemoryEntry oldEntry = snapshot.Structure.GetMemoryEntry(index);
                    HashSetTools.AddAll(newValues, oldEntry.PossibleValues);
                }

                HashSetTools.AddAll(newValues, entry.PossibleValues);

                snapshot.Structure.SetMemoryEntry(index, entry);
            }
        }
    }
}
