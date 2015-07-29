using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Utils
{
    class ValueUtils
    {
        /// <summary>
        /// Compares the memory entries to determine whether are the same. 
        /// 
        /// ID af associated values are not compared. Just compares the presence of the array.
        /// </summary>
        /// <param name="newEntry">The new entry.</param>
        /// <param name="oldEntry">The old entry.</param>
        /// <returns>True if compared entries contains the same values; otherwise false.</returns>
        public static bool CompareMemoryEntries(MemoryEntry newEntry, MemoryEntry oldEntry)
        {
            if (newEntry == oldEntry)
            {
                return true;
            }

            if (newEntry == null || oldEntry == null)
            {
                return false;
            }

            if (newEntry.Count != oldEntry.Count)
            {
                return false;
            }

            if (newEntry.ContainsAssociativeArray != oldEntry.ContainsAssociativeArray)
            {
                return false;
            }

            HashSet<Value> oldValues = new HashSet<Value>(oldEntry.PossibleValues);
            foreach (Value value in newEntry.PossibleValues)
            {
                if (!(value is AssociativeArray))
                {
                    if (!oldValues.Contains(value))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
