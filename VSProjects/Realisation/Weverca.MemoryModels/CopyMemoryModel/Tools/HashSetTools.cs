using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class HashSetTools
    {
        public static void AddAll<T>(ICollection<T> targetSet, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                targetSet.Add(value);
            }
        }

        public static bool EqualsSet<T>(ICollection<T> setA, ICollection<T> setB)
        {
            if (setA == setB)
            {
                return true;
            }

            if (setA.Count != setB.Count)
            {
                return false;
            }

            foreach (T value in setA)
            {
                if (!setB.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
