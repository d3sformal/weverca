using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.LazyStructure
{
    class DifferentialDictionaryAssociativeContainerFactory : IAssociativeContainerFactory
    {
        public IWriteableAssociativeContainer<TKey, TValue> CreateWriteableAssociativeContainer<TKey, TValue>()
        {
            return new DifferentialDictionaryAssociativeContainer<TKey, TValue>();
        }
    }


    class DifferentialDictionaryAssociativeContainer<TKey, TValue> : IWriteableAssociativeContainer<TKey, TValue>
    {
        public static readonly int NodeSequenceLimit = 30;
        public static readonly int RootMergeLimit = 100;
        
        private class ContainerNode
        {
            public readonly int NumberInSequence;
            public readonly ContainerNode PreviousNode;

            public readonly Dictionary<TKey, ContainerValue> AssociativeContainer;

            public ContainerNode()
            {
                AssociativeContainer = new Dictionary<TKey, ContainerValue>();
                PreviousNode = null;
                NumberInSequence = 0;
            }

            public ContainerNode(Dictionary<TKey, ContainerValue> container)
            {
                AssociativeContainer = container;
                PreviousNode = null;
                NumberInSequence = 0;
            }

            public ContainerNode(ContainerNode previousNode)
            {
                AssociativeContainer = new Dictionary<TKey, ContainerValue>();
                PreviousNode = previousNode;
                NumberInSequence = previousNode.NumberInSequence + 1;
            }
        }

        private class ContainerValue
        {
            public readonly TValue Value;
            public readonly bool HasValue;

            public ContainerValue()
            {
                HasValue = false;
            }

            public ContainerValue(TValue value)
            {
                Value = value;
                HasValue = true;
            }
        }

        Dictionary<TKey, ContainerValue> rootContainer;
        ContainerNode node = null;
        bool copied = false;

        public DifferentialDictionaryAssociativeContainer()
        {
            node = new ContainerNode();
            rootContainer = new Dictionary<TKey, ContainerValue>();
        }

        public DifferentialDictionaryAssociativeContainer(DifferentialDictionaryAssociativeContainer<TKey, TValue> cloned)
        {
            node = cloned.node;
            rootContainer = cloned.rootContainer;
            copied = false;
        }

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

        public void Add(TKey key, TValue value)
        {
            copy();
            node.AssociativeContainer[key] = new ContainerValue(value);
        }

        public void Remove(TKey key)
        {
            copy();
            node.AssociativeContainer[key] = new ContainerValue();
        }

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

        public void Clear()
        {
            node = new ContainerNode();
            copied = true;
        }

        public IEnumerable<TKey> Keys
        {
            get { return getAllKeys(); }
        }

        public int Count
        {
            get { return getAllKeys().Count; }
        }

        public bool ContainsKey(TKey key)
        {
            TValue value;
            return TryGetValue(key, out value);
        }

        public IWriteableAssociativeContainer<TKey, TValue> Copy()
        {
            return new DifferentialDictionaryAssociativeContainer<TKey, TValue>(this);
        }

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

        private void joinNodes()
        {

            Dictionary<TKey, ContainerValue> joinedContainer = new Dictionary<TKey, ContainerValue>();
            HashSet<TKey> removedKeys = new HashSet<TKey>();

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
    }
}
