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
        }

        private void assignMust(MemoryIndex mustIndex, CollectComposedValuesVisitor composedValues)
        {
            IndexData data = snapshot.Data.GetIndexData(mustIndex);
            HashSet<Value> values = new HashSet<Value>(composedValues.Values);

            if (data.Array != null)
            {
                values.Add(data.Array);
            }

            if (composedValues.Objects.Count > 0)
            {
                snapshot.Data.SetObjects(mustIndex, new ObjectValueContainer(composedValues.Objects));
                HashSetTools.AddAll(values, data.Objects);
            }

            snapshot.Data.SetMemoryEntry(mustIndex, new MemoryEntry(values));
        }

        private void assignMay(MemoryIndex mayIndex, CollectComposedValuesVisitor composedValues)
        {
            IndexData data = snapshot.Data.GetIndexData(mayIndex);
            HashSet<Value> values = new HashSet<Value>(composedValues.Values);
            
            if (composedValues.Objects.Count > 0)
            {
                HashSet<ObjectValue> objects = new HashSet<ObjectValue>(data.Objects);
                HashSetTools.AddAll(objects, composedValues.Objects);
                snapshot.Data.SetObjects(mayIndex, new ObjectValueContainer(objects));

                HashSetTools.AddAll(values, data.Objects);
            }

            HashSetTools.AddAll(values, data.MemoryEntry.PossibleValues);
            snapshot.Data.SetMemoryEntry(mayIndex, new MemoryEntry(values));
        }
    }
}
