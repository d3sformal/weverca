using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PHP.Core.ControlFlow
{
    public interface IReadOnlyArray<T> : IEnumerable<T>
    {
        T this[int index] { get; }

        int Length { get; }
    }

    public class ReadOnlyList<T> : IReadOnlyArray<T>
    {
        private IList<T> data;

        public ReadOnlyList(IList<T> data)
        {
            this.data = data;
        }

        public T this[int index] { get { return this.data[index]; } }

        public int Length { get { return this.data.Count; } }

        public IEnumerator<T> GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)this.data).GetEnumerator();
        }
    }

    public static class ReadOnlyArray
    {
        public static IReadOnlyArray<T> Create<T>(IList<T> data)
        {
            return new ReadOnlyList<T>(data);
        }
    }
}
