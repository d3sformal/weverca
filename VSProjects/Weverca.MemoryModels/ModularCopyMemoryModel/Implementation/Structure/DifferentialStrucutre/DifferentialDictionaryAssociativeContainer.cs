using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.DifferentialStructure
{
    class DifferentialDictionaryAssociativeContainerFactory : IAssociativeContainerFactory
    {
        public IWriteableAssociativeContainer<TKey, TValue> CreateWriteableAssociativeContainer<TKey, TValue>()
        {
            return new DifferentialDictionaryAssociativeContainer<TKey, TValue>();
        }
    }


    /// <summary>
    /// Differential implementation of an associative container.
    /// 
    /// Differential containers are another way how to reduce the number of copying in lazy container. 
    /// Each version of a differential container contains a link to its ancestor and a list of changes. 
    /// Reading means to find proper container with the last version of the value or information that 
    /// the value was deleted. 
    /// 
    /// Complexity of a read is reduced by using a limit for number of containers in a row. The data 
    /// are merged to a single container when the length of the sequence exceeds the specified limit.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    class DifferentialDictionaryAssociativeContainer<TKey, TValue> : IWriteableAssociativeContainer<TKey, TValue>
    {
        public static readonly int NodeSequenceLimit = 30;
        public static readonly int RootMergeLimit = 100;

        /// <summary>
        /// Represents an inner associative conatiner with link to the previous node.It is a linked
        /// list of dictionaries. Each dictionary contains the changes up to the previous node.
        /// </summary>
        private class ContainerNode
        {
            /// <summary>
            /// Position of the node within the list - lengt of a path to the root
            /// </summary>
            public readonly int NumberInSequence;
            
            /// <summary>
            /// Link to the previous node in the linked list. Null if node is root.
            /// </summary>
            public readonly ContainerNode PreviousNode;

            /// <summary>
            /// Inner dictionary with current data
            /// </summary>
            public readonly Dictionary<TKey, ContainerValue> AssociativeContainer;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerNode"/> class. New node will 
            /// have no ancestor - it will be a root.
            /// </summary>
            public ContainerNode()
            {
                AssociativeContainer = new Dictionary<TKey, ContainerValue>();
                PreviousNode = null;
                NumberInSequence = 0;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerNode"/> class. Uses given
            /// dictionary as the inner container. Node will be a root.
            /// </summary>
            /// <param name="container">The container.</param>
            public ContainerNode(Dictionary<TKey, ContainerValue> container)
            {
                AssociativeContainer = container;
                PreviousNode = null;
                NumberInSequence = 0;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerNode"/> class. New instance
            /// will be inserted to the linked list after the given node.
            /// </summary>
            /// <param name="previousNode">The previous node.</param>
            public ContainerNode(ContainerNode previousNode)
            {
                AssociativeContainer = new Dictionary<TKey, ContainerValue>();
                PreviousNode = previousNode;
                NumberInSequence = previousNode.NumberInSequence + 1;
            }
        }

        /// <summary>
        /// Holds the value stored within the container. Represents the value or information that 
        /// value was deleted.
        /// </summary>
        private class ContainerValue
        {
            public readonly TValue Value;
            public readonly bool HasValue;

            /// <summary>
            /// Initializes a new tombstone instance of the <see cref="ContainerValue"/> class.
            /// </summary>
            public ContainerValue()
            {
                HasValue = false;
            }

            /// <summary>
            /// Initializes a new instance with value of the <see cref="ContainerValue"/> class.
            /// </summary>
            /// <param name="value">The value.</param>
            public ContainerValue(TValue value)
            {
                Value = value;
                HasValue = true;
            }
        }

        Dictionary<TKey, ContainerValue> rootContainer;
        ContainerNode node = null;
        bool copied = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentialDictionaryAssociativeContainer{TKey, TValue}"/> class.
        /// </summary>
        public DifferentialDictionaryAssociativeContainer()
        {
            node = new ContainerNode();
            rootContainer = new Dictionary<TKey, ContainerValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentialDictionaryAssociativeContainer{TKey, TValue}"/> class.
        /// Given instance is lazy copied.
        /// </summary>
        /// <param name="cloned">The cloned.</param>
        public DifferentialDictionaryAssociativeContainer(DifferentialDictionaryAssociativeContainer<TKey, TValue> cloned)
        {
            node = cloned.node;
            rootContainer = cloned.rootContainer;
            copied = false;
        }

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The value with the specified key</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Assocoative container does not contain given key.</exception>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException("Assocoative container does not contain key: " + key.ToString());
                }
            }
            set
            {
                Add(key, value);
            }
        }


        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            copy();
            node.AssociativeContainer[key] = new ContainerValue(value);
        }

        /// <summary>
        /// Removes the specified key from dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(TKey key)
        {
            copy();
            node.AssociativeContainer[key] = new ContainerValue();
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns> true if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);

            ContainerNode accNode = node;
            
            while (accNode != null)
            {
                ContainerValue containerValue;
                if (accNode.AssociativeContainer.TryGetValue(key, out containerValue))
                {
                    if (containerValue.HasValue)
                    {
                        value = containerValue.Value;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                accNode = accNode.PreviousNode;
            }

            ContainerValue rootValue;
            if (rootContainer.TryGetValue(key, out rootValue))
            {
                value = rootValue.Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public void Clear()
        {
            node = new ContainerNode();
            copied = true;
        }

        /// <summary>
        /// Gets the list of all keys stored in the dictionary.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<TKey> Keys
        {
            get { return getAllKeys(); }
        }

        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get { return getAllKeys().Count; }
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the dictionary contains the specified key; otherwise false</returns>
        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }


        /// <summary>
        /// Creates new instance and copy all data from this container.
        /// </summary>
        /// <returns>New instance with all data from this container.</returns>
        public IWriteableAssociativeContainer<TKey, TValue> Copy()
        {
            return new DifferentialDictionaryAssociativeContainer<TKey, TValue>(this);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            HashSet<TKey> used = new HashSet<TKey>();
            List<KeyValuePair<TKey, TValue>> values = new List<KeyValuePair<TKey, TValue>>();

            ContainerNode accNode = node;
            while (accNode != null)
            {
                foreach (var item in accNode.AssociativeContainer)
                {
                    if (!used.Contains(item.Key))
                    {
                        if (item.Value.HasValue)
                        {
                            values.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value.Value));
                        }
                        used.Add(item.Key);
                    }
                }

                accNode = accNode.PreviousNode;
            }

            foreach (var item in rootContainer)
            {
                if (!used.Contains(item.Key) && item.Value.HasValue)
                {
                    values.Add(new KeyValuePair<TKey, TValue>(item.Key, item.Value.Value));
                }
            }

            return values.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void copy()
        {
            if (!copied)
            {
                if (node.NumberInSequence < NodeSequenceLimit)
                {
                    node = new ContainerNode(node);
                }
                else
                {
                    joinNodes();
                }

                copied = true;
            }
        }

        /// <summary>
        /// This method merges all values within the linked list to the single root node.
        /// </summary>
        private void joinNodes()
        {

            Dictionary<TKey, ContainerValue> joinedContainer = new Dictionary<TKey, ContainerValue>();
            HashSet<TKey> removedKeys = new HashSet<TKey>();

            // collects all values within the list
            ContainerNode accNode = node;
            while (accNode != null)
            {
                foreach (var item in accNode.AssociativeContainer)
                {
                    if (!removedKeys.Contains(item.Key) && !joinedContainer.ContainsKey(item.Key))
                    {
                        if (item.Value.HasValue)
                        {
                            joinedContainer.Add(item.Key, item.Value);
                        }
                        else if (rootContainer.ContainsKey(item.Key))
                        {
                            joinedContainer.Add(item.Key, item.Value);
                        }
                        else
                        {
                            removedKeys.Add(item.Key);
                        }
                    }
                }

                accNode = accNode.PreviousNode;
            }

            // If the joinded container is bigger than specified limit then merge with the root container.
            if (joinedContainer.Count > RootMergeLimit)
            {
                foreach (var item in rootContainer) 
                {
                    ContainerValue value;
                    if (joinedContainer.TryGetValue(item.Key, out value))
                    {
                        if (!value.HasValue)
                        {
                            joinedContainer.Remove(item.Key);
                        }
                    }
                    else
                    {
                        joinedContainer.Add(item.Key, item.Value);
                    }
                }

                rootContainer = joinedContainer;
                node = new ContainerNode();
            }
            else
            {
                node = new ContainerNode(joinedContainer);
            }
        }

        private HashSet<TKey> getAllKeys()
        {
            HashSet<TKey> alive = new HashSet<TKey>();
            HashSet<TKey> removed = new HashSet<TKey>();
            ContainerNode accNode = node;
            while (accNode != null)
            {
                foreach (var item in accNode.AssociativeContainer)
                {
                    if (!removed.Contains(item.Key) && !alive.Contains(item.Key))
                    {
                        if (item.Value.HasValue)
                        {
                            alive.Add(item.Key);
                        }
                        removed.Add(item.Key);
                    }
                }

                accNode = accNode.PreviousNode;
            }

            foreach (var item in rootContainer)
            {
                if (!removed.Contains(item.Key) && !alive.Contains(item.Key))
                {
                    alive.Add(item.Key);
                }
            }

            return alive;
        }
    }
}
