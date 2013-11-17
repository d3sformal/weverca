using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
