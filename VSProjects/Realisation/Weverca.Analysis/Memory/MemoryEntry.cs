using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Memory entry represents multiple possiblities that for example single variable can have
    /// NOTE:
    ///     * Is immutable    
    /// </summary>
    public class MemoryEntry
    {
        public readonly IEnumerable<Value> PossibleValues;

        public MemoryEntry(params Value[] values)
        {
            PossibleValues = new ReadOnlyCollection<Value>((Value[])values.Clone());
        }

        /// <summary>
        /// Is used for avoiding of copy on internall structures 
        /// </summary>
        /// <param name="values">Values that will be passed into memory entry - no copy is proceeded</param>
        private MemoryEntry(IEnumerable<Value> values)
        {
            PossibleValues = values;
        }
        /// <summary>
        /// Shallow memory entries merge
        /// Distincity is reached by Equals and GetHashCode comparison.
        /// </summary>
        /// <param name="entries">Memory entries to be merged</param>
        /// <returns>Memory entry containing disctinct values from all entries</returns>
        public static MemoryEntry Merge(params MemoryEntry[] entries)
        {
            return Merge((IEnumerable<MemoryEntry>)entries);
        }

        public static MemoryEntry Merge(IEnumerable<MemoryEntry> entries)
        {
            var values = new HashSet<Value>();

            foreach (var entry in entries)
            {
                values.UnionWith(entry.PossibleValues);
            }

            return new MemoryEntry(values);
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var o = obj as MemoryEntry;

            if (o == null)
            {
                return false;
            }

            return hasSameValues(o.PossibleValues, this.PossibleValues);
        }

        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var val in PossibleValues)
            {
                hashCode += val.GetHashCode();
            }

            return hashCode;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append("Values: ");
            foreach (var value in PossibleValues)
            {
                result.AppendFormat("({0}),", value.ToString());
            }

            result.Length--;

            return result.ToString();
        }

        private bool hasSameValues(IEnumerable<Value> values1, IEnumerable<Value> values2)
        {
            var set1 = new HashSet<Value>(values1);
            var values2Cn = 0;
            foreach (var value in values2)
            {
                if (!set1.Contains(value))
                {
                    return false;
                }
                ++values2Cn;
            }

            return set1.Count == values2Cn;
        }

    }
}
