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
