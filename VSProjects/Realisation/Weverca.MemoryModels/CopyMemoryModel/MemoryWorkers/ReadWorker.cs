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
    /// Algorithm which retrieves all values from every memory locations specified by given collector.
    /// As output value analysis retreive memory entry which contains concatenated values. Output memory
    /// entry also can contain several arrays from several locations. These arrays are not merged together
    /// and analysis can process each array separatly. Merge of arrays is provided just when it is needed
    /// on assign memory entry to new memory location.
    /// </summary>
    class ReadWorker
    {
        private Snapshot snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWorker"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public ReadWorker(Snapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Reads the value from locations specified by given collector.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <returns></returns>
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
                foreach (ValueLocation location in collector.MustLocation)
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
        
        /// <summary>
        /// Value lovation visitor to process value locations in info level phase.
        /// As the output of visitor can be read memory entry which is stored in memory index associated with visited value.
        /// </summary>
        class InfoLocationVisitor : IValueLocationVisitor
        {
            /// <summary>
            /// Gets or the value which was read by this visitor.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public IEnumerable<Value> Value { get; private set; }

            Snapshot snapshot;

            /// <summary>
            /// Initializes a new instance of the <see cref="InfoLocationVisitor"/> class.
            /// </summary>
            /// <param name="snapshot">The snapshot.</param>
            public InfoLocationVisitor(Snapshot snapshot)
            {
                this.snapshot = snapshot;
            }

            /// <summary>
            /// Visits the object value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectValueLocation(ObjectValueLocation location)
            {
            }

            /// <summary>
            /// Visits the object any value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectAnyValueLocation(ObjectAnyValueLocation location)
            {
                read(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayValueLocation(ArrayValueLocation location)
            {
                read(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array any value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayAnyValueLocation(ArrayAnyValueLocation location)
            {
                read(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array string value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayStringValueLocation(ArrayStringValueLocation location)
            {
                read(location.ContainingIndex);
            }

            /// <summary>
            /// Visits the array undefined value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitArrayUndefinedValueLocation(ArrayUndefinedValueLocation location)
            {
            }

            /// <summary>
            /// Visits the object undefined value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
            {
            }

            /// <summary>
            /// Visits the information value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitInfoValueLocation(InfoValueLocation location)
            {
            }

            /// <summary>
            /// Visits any string value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitAnyStringValueLocation(AnyStringValueLocation location)
            {
                read(location.ContainingIndex);
            }

            /// <summary>
            /// Reads the values which is store in specified index.
            /// </summary>
            /// <param name="index">The index.</param>
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
