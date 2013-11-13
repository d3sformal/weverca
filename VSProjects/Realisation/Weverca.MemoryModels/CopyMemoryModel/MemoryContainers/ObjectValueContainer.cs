using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    public class ObjectValueContainer : IEnumerable<ObjectValue>
    {
        HashSet<ObjectValue> values;

        public int Count { get { return values.Count; } }

        public ObjectValueContainer()
        {
            values = new HashSet<ObjectValue>();
        }

        public ObjectValueContainer(ObjectValueContainerBuilder builder)
        {
            values = new HashSet<ObjectValue>(builder.Values);
        }

        public ObjectValueContainer(IEnumerable<ObjectValue> values)
        {
            this.values = new HashSet<ObjectValue>(values);
        }

        public bool Contains(ObjectValue value)
        {
            return values.Contains(value);
        }

        public ObjectValueContainerBuilder Builder()
        {
            return new ObjectValueContainerBuilder(this);
        }

        public IEnumerator<ObjectValue> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }
    }

    public class ObjectValueContainerBuilder : IEnumerable<ObjectValue>
    {
        public HashSet<ObjectValue> Values { get; private set; }

        public ObjectValueContainerBuilder(ObjectValueContainer objectValueContainer)
        {
            Values = new HashSet<ObjectValue>(objectValueContainer);
        }

        public void Add(ObjectValue value)
        {
            Values.Add(value);
        }
        public void Remove(ObjectValue value)
        {
            Values.Remove(value);
        }
        public bool Contains(ObjectValue value)
        {
            return Values.Contains(value);
        }
        public void Clear()
        {
            Values.Clear();
        }

        public IEnumerator<ObjectValue> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public ObjectValueContainer Build()
        {
            return new ObjectValueContainer(this);
        }
    }
}
