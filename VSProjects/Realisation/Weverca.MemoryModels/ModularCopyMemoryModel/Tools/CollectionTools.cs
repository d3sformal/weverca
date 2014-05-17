using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Tools
{
    /// <summary>
    /// Static tool class. Contains generic static helper methods for sets and collections.
    /// </summary>
    public class CollectionTools
    {
        /// <summary>
        /// Adds all values to target collection.
        /// </summary>
        /// <typeparam name="T">Type of collection.</typeparam>
        /// <param name="targetSet">The target set.</param>
        /// <param name="values">The values.</param>
        public static void AddAll<T>(ICollection<T> targetSet, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                targetSet.Add(value);
            }
        }

        /// <summary>
        /// Determines whether given collections contains the same values or not.
        /// </summary>
        /// <typeparam name="T">Type of collection.</typeparam>
        /// <param name="setA">The set a.</param>
        /// <param name="setB">The set b.</param>
        /// <returns>True whethe given collections contains the same values.</returns>
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

            foreach (T value in setB)
            {
                if (!setA.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
