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
        /// Reads the values from locations specified by given collector.
        /// </summary>
        /// <param name="collector">The collector.</param>
        /// <returns>Memory entry with values from locations specified by given collector.</returns>
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

                foreach (ValueLocation location in collector.MustLocation)
                {
                    if (snapshot.CurrentMode == SnapshotMode.MemoryLevel)
                    {
                        HashSetTools.AddAll(values, location.ReadValues(snapshot.MemoryAssistant));
                    }
                    else
                    {
                        InfoLocationVisitor visitor = new InfoLocationVisitor(snapshot);
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
                Value = new Value[] { };
            }

            /// <summary>
            /// Visits the object undefined value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitObjectUndefinedValueLocation(ObjectUndefinedValueLocation location)
            {
                Value = new Value[] { };
            }

            /// <summary>
            /// Visits the information value location.
            /// </summary>
            /// <param name="location">The location.</param>
            public void VisitInfoValueLocation(InfoValueLocation location)
            {
                read(location.ContainingIndex);
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