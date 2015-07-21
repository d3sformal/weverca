using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common
{
    public interface IAssociativeContainerFactory
    {
        IWriteableAssociativeContainer<TKey, TValue> CreateWriteableAssociativeContainer<TKey, TValue>();
    }

    public interface IReadonlyAssociativeContainer<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        int Count { get; }

        IEnumerable<TKey> Keys { get; }
        
        TValue this[TKey key] { get; }

        bool ContainsKey(TKey key);

        IWriteableAssociativeContainer<TKey, TValue> Copy();
    }

    public interface IWriteableAssociativeContainer<TKey, TValue> : IReadonlyAssociativeContainer<TKey, TValue>
    {
        TValue this[TKey key] { get; set; }

        void Add(TKey key, TValue value);

        void Remove(TKey key);

        bool TryGetValue(TKey key, out TValue value);

        void Clear();
    }
}
