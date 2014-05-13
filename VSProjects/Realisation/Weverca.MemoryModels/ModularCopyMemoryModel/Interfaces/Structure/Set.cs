using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.CopyMemoryModel;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Represents readonly version of set to store collection ofdistinct values.
    /// </summary>
    /// <typeparam name="T">Tye of value to store in set.</typeparam>
    public interface IReadonlySet<T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets the collection of values stored in the set.
        /// </summary>
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Gets the number of values in set.
        /// </summary>
        /// <value>
        /// The number of values in set.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Determines whether the set contains specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True whether the set contains specified value.</returns>
        bool Contains(T value);
    }

    /// <summary>
    /// Represents writeable version of set to store collection ofdistinct values.
    /// </summary>
    /// <typeparam name="T">Tye of value to store in set.</typeparam>
    public interface IWriteableSet<T> : IReadonlySet<T>
    {
        /// <summary>
        /// Adds the value into set.
        /// </summary>
        /// <param name="value">The value.</param>
        void Add(T value);

        /// <summary>
        /// Adds all values into set.
        /// </summary>
        /// <param name="mustAliases">Collection of values.</param>
        void AddAll(IEnumerable<T> values);

        /// <summary>
        /// Removes the value from set.
        /// </summary>
        /// <param name="value">The value.</param>
        void Remove(T value);

        /// <summary>
        /// Removes all values from set.
        /// </summary>
        void Clear();
    }
}
