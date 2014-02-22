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
    class ReadWorker
    {
        private Snapshot snapshot;


        public ReadWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        public MemoryEntry ReadValue(IIndexCollector collector)
        {
            if (collector.MustIndexesCount == 1 && collector.IsDefined)
            {
                MemoryIndex index = collector.MustIndexes.First();
                return snapshot.Structure.GetMemoryEntry(index);
            }
            else
            {
                HashSet<Value> values = new HashSet<Value>();
                if (!collector.IsDefined)
                {
                    values.Add(snapshot.UndefinedValue);
                }

                foreach (MemoryIndex index in collector.MustIndexes)
                {
                    MemoryEntry entry = snapshot.Structure.GetMemoryEntry(index);
                    HashSetTools.AddAll(values, entry.PossibleValues);
                }

                InfoLocationVisitor visitor = new InfoLocationVisitor(snapshot);
                foreach (CollectedLocation location in collector.MustLocation)
                {
                    if (snapshot.CurrentMode == SnapshotMode.MemoryLevel)
                    {
                        HashSetTools.AddAll(values, location.ReadValues(snapshot.MemoryAssistant));
                    }
                    else
                    {
                        location.Accept(visitor);
                        HashSetTools.AddAll(values, visitor.Value);
                    }
                }

                return new MemoryEntry(values);
            }

        }

        class InfoLocationVisitor : ICollectedLocationVisitor
        {
            public IEnumerable<Value> Value { get; private set; }

            Snapshot snapshot;
            public InfoLocationVisitor(Snapshot snapshot)
            {
                this.snapshot = snapshot;
            }

            public void VisitObjectValueLocation(ObjectValueLocation location)
            {
            }

            public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
            {
                read(location.ContainingIndex);
            }

            public void VisitArrayValueLocation(ArrayValueLocation location)
            {
                read(location.ContainingIndex);
            }

            public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
            {
                read(location.ContainingIndex);
            }

            public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
            {
                read(location.ContainingIndex);
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
                read(location.ContainingIndex);
            }

            private void read(MemoryIndex index)
            {
                MemoryEntry entry;
                if (snapshot.Structure.TryGetMemoryEntry(index, out entry))
                {
                    Value = entry.PossibleValues;
                }
                else
                {
                    Value = new Value[] { };
                }
            }

        }
    }
}
