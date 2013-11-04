using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Memory entry represents multiple possibilities that for example single variable can have
    /// NOTE:
    ///     * Is immutable
    /// </summary>
    public class MemoryEntry
    {
        /// <summary>
        /// Values that are possible for current memory entry
        /// </summary>
        public readonly IEnumerable<Value> PossibleValues;

        /// <summary>
        /// Count of possible values in current memory entry
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryEntry" /> class.
        /// Create memory entry from given values.
        /// NOTE:
        ///     * Values has to be distinct
        /// </summary>
        /// <param name="values">Possible values for created memory entry</param>
        public MemoryEntry(params Value[] values) :
            this(values.Clone() as Value[], false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryEntry" /> class.
        /// Create memory entry from given values.
        /// NOTE:
        ///     * Values has to be distinct
        /// </summary>
        /// <param name="values">Possible values for created memory entry</param>
        public MemoryEntry(IEnumerable<Value> values) :
            this(new List<Value>(values).ToArray(), false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryEntry" /> class.
        /// Is used for avoiding of copy on internal structures
        /// </summary>
        /// <param name="values">Values that will be passed into memory entry - no copy is proceeded</param>
        /// <param name="copy">Determine that copy has to be created for values</param>
        private MemoryEntry(Value[] values, bool copy)
        {
            if (copy)
            {
                var copiedValues = new List<Value>();
                copiedValues.AddRange(values);
                values = copiedValues.ToArray();
            }

            PossibleValues = new ReadOnlyCollection<Value>(values);
            Count = values.Length;
        }

        /// <summary>
        /// Shallow memory entries merge
        /// Distinction is reached by Equals and GetHashCode comparison.
        /// </summary>
        /// <param name="entries">Memory entries to be merged</param>
        /// <returns>Memory entry containing distinct values from all entries</returns>
        public static MemoryEntry Merge(params MemoryEntry[] entries)
        {
            return Merge(entries as IEnumerable<MemoryEntry>);
        }

        /// <summary>
        /// Shallow memory entries merge
        /// NOTE:
        ///    * Distinction is reached by Equals and GetHashCode comparison of enumerated values.
        /// </summary>
        /// <param name="entries">Memory entries to be merged</param>
        /// <returns>Memory entry containing distinct values from all entries</returns>
        public static MemoryEntry Merge(IEnumerable<MemoryEntry> entries)
        {
            var values = new HashSet<Value>();

            foreach (var entry in entries)
            {
                values.UnionWith(entry.PossibleValues);
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current memory entry.
        /// </summary>
        /// <param name="obj">The object to compare with the current memory entry</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current memory entry, otherwise <c>false</c>
        /// </returns>
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

        /// <summary>
        /// Returns the hash code calculated from all possible values in current memory entry.
        /// </summary>
        /// <returns>A hash code for the current memory entry</returns>
        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var value in PossibleValues)
            {
                hashCode += value.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// Returns a string consisting of string representations of every possible value
        /// in current memory entry.
        /// </summary>
        /// <returns>A string that represents the current memory entry</returns>
        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append("Values: ");
            foreach (var value in PossibleValues)
            {
                result.AppendFormat("({0}),", value.ToString());
            }

            result.Length--;

            return result.ToString();
        }

        /// <summary>
        /// Determines whether the first and second enumeration contain same values
        /// (according to their equals and hash code)
        /// </summary>
        /// <param name="values1">First values enumeration</param>
        /// <param name="values2">Second values enumeration</param>
        /// <returns><c>true</c> if values are same (in any order), <c>false</c> otherwise</returns>
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
