using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common
{
    /// <summary>
    /// Instances of this factory class are used to create the new empty writeable associative 
    /// container which implements IWriteableAssociativeContainer.
    /// </summary>
    public interface IAssociativeContainerFactory
    {
        /// <summary>
        /// Creates the new empty writeable associative container.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns>T new empty writeable associative container.</returns>
        IWriteableAssociativeContainer<TKey, TValue> CreateWriteableAssociativeContainer<TKey, TValue>();
    }

    /// <summary>
    /// Represents a readonly version of an associative container used in the memory model.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IReadonlyAssociativeContainer<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Gets the number of elements within this instance.
        /// </summary>
        /// <value>
        /// The number of elements within this instance.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the collectio of all keys presented in this container.
        /// </summary>
        /// <value>
        /// The collectio of all keys stored in this container.
        /// </value>
        IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// Gets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>Value stored with the specified key.</returns>
        TValue this[TKey key] { get; }

        /// <summary>
        /// Determines whether the collection contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the collection contains the specified key; otherwise false.</returns>
        bool ContainsKey(TKey key);

        /// <summary>
        /// Creates new instance and copy all data from this container.
        /// </summary>
        /// <returns>New instance with all data from this container.</returns>
        IWriteableAssociativeContainer<TKey, TValue> Copy();

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the container contains an element with the specified key; otherwise, false.</returns>
        bool TryGetValue(TKey key, out TValue value);
    }

    /// <summary>
    /// Represents a writeable version of an associative container used in the memory model.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IWriteableAssociativeContainer<TKey, TValue> : IReadonlyAssociativeContainer<TKey, TValue>
    {
        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>Value stored with the specified key.</returns>
        TValue this[TKey key] { set; }

        /// <summary>
        /// Adds new value with the specified key to the container.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void Add(TKey key, TValue value);

        /// <summary>
        /// Removes the specified key from the container.
        /// </summary>
        /// <param name="key">The key.</param>
        void Remove(TKey key);

        /// <summary>
        /// Removes all keys and values from this container.
        /// </summary>
        void Clear();
    }
}
