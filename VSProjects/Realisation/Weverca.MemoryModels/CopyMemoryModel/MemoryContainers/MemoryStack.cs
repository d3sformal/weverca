using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    class MemoryStack<T> : IEnumerable<T> where T : IGenericCloneable<T>
    {
        private readonly T[] stack;
        private readonly int localIndex;

        public int Length { get { return stack.Length; } }

        public T Local { get { return stack[localIndex]; } }
        public T Global { get { return stack[0]; } }

        public MemoryStack(T local)
        {
            localIndex = 0;
            stack = new T[] { local };
        }

        public MemoryStack(MemoryStack<T> oldStack)
        {
            localIndex = oldStack.Length - 1;

            stack = new T[oldStack.Length];
            copy(oldStack.stack, this.stack, oldStack.Length);
        }

        public MemoryStack(MemoryStack<T> oldStack, T local)
        {
            localIndex = oldStack.Length;

            stack = new T[oldStack.Length + 1];
            copy(oldStack.stack, this.stack, oldStack.Length);

            stack[oldStack.Length] = local;
        }

        public MemoryStack(int callLevel)
        {
            stack = new T[callLevel + 1];
            localIndex = callLevel;
        }

        public T this[int index]
        {
            get { return stack[index]; }
            set { stack[index] = value; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)stack).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)stack).GetEnumerator();
        }

        private void copy(T[] source, T[] target, int length)
        {
            for (int x = 0; x < length; x++)
            {
                target[x] = source[x].Clone();
            }
        }
    }
}
