/*
Copyright (c) 2012-2014 Pavel Bastecky.

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Implmentation of stack which contains cloneable containers. Used to model memory call stack
    /// of variables. Every instance can contain collections of indexes for each call level. Memory model
    /// then can alasyly acces global and local level of memory stack. Once the stack is created the number
    /// of levels can not be changed. However class also provides deep copy functionality or allows 
    /// to add new stack level after copying.
    /// </summary>
    /// <typeparam name="T">Type of collection in the stack. Has to implement cloneable interface.</typeparam>
    public class MemoryStack<T> : IEnumerable<T> where T : IGenericCloneable<T>
    {
        /// <summary>
        /// The collection of object stored in stack.
        /// Each index is one level of memory stack - zero is global and the last is local level.
        /// </summary>
        protected readonly T[] stack;

        /// <summary>
        /// The index of local level in memory stack
        /// </summary>
        private readonly int localIndex;

        /// <summary>
        /// Gets the length of memory stack.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get { return stack.Length; } }

        /// <summary>
        /// Gets the local level of memory stack.
        /// </summary>
        /// <value>
        /// The local level.
        /// </value>
        public T Local { get { return stack[localIndex]; } }

        /// <summary>
        /// Gets the global level of memory stack.
        /// </summary>
        /// <value>
        /// The global level.
        /// </value>
        public T Global { get { return stack[0]; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStack{T}"/> class.
        /// </summary>
        /// <param name="local">The local.</param>
        public MemoryStack(T local)
        {
            localIndex = 0;
            stack = new T[] { local };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStack{T}"/> class.
        /// </summary>
        /// <param name="oldStack">The old stack.</param>
        public MemoryStack(MemoryStack<T> oldStack)
        {
            localIndex = oldStack.Length - 1;

            stack = new T[oldStack.Length];
            copy(oldStack.stack, this.stack, oldStack.Length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStack{T}"/> class.
        /// </summary>
        /// <param name="oldStack">The old stack.</param>
        /// <param name="local">The local.</param>
        public MemoryStack(MemoryStack<T> oldStack, T local)
        {
            localIndex = oldStack.Length;

            stack = new T[oldStack.Length + 1];
            copy(oldStack.stack, this.stack, oldStack.Length);

            stack[oldStack.Length] = local;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryStack{T}"/> class.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        public MemoryStack(int callLevel)
        {
            stack = new T[callLevel + 1];
            localIndex = callLevel;
        }

        /// <summary>
        /// Gets or sets the stack container at the level given by specified index.
        /// </summary>
        /// <value>
        /// The stack container.
        /// </value>
        /// <param name="index">The index of stack level.</param>
        /// <returns>Stack object on the specified level.</returns>
        public T this[int index]
        {
            get { return stack[index]; }
            set { stack[index] = value; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)stack).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)stack).GetEnumerator();
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="length">The length.</param>
        private static void copy(T[] source, T[] target, int length)
        {
            for (int x = 0; x < length; x++)
            {
                target[x] = source[x].Clone();
            }
        }
    }

    /// <summary>
    /// Memory stack which is used to store memory indexes in IndexContainer instances. This class
    /// is used for storing variables and theirs indexes. Implementation also adds routines for converting
    /// memory stack to string.
    /// </summary>
    class VariableStack : MemoryStack<IndexContainer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableStack"/> class.
        /// </summary>
        /// <param name="local">The local.</param>
        public VariableStack(IndexContainer local)
            : base(local)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableStack"/> class.
        /// </summary>
        /// <param name="oldStack">The old stack.</param>
        public VariableStack(MemoryStack<IndexContainer> oldStack)
            : base(oldStack)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableStack"/> class.
        /// </summary>
        /// <param name="oldStack">The old stack.</param>
        /// <param name="local">The local.</param>
        public VariableStack(MemoryStack<IndexContainer> oldStack, IndexContainer local)
            : base(oldStack, local)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableStack"/> class.
        /// </summary>
        /// <param name="callLevel">The call level.</param>
        public VariableStack(int callLevel)
            : base(callLevel)
        {
        }

        /// <summary>
        /// Gets the string representation of local level of stack.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <returns>String representation of local variables.</returns>
        internal string GetLocalRepresentation(SnapshotData data, SnapshotData infos)
        {
            StringBuilder result = new StringBuilder();
            for (int x = 1; x < stack.Length; x++)
            {
                stack[x].GetRepresentation(data, infos, result);
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets the string representation of global level of stack.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="infos">The infos.</param>
        /// <returns>String representation of global variables.</returns>
        internal string GetGlobalRepresentation(SnapshotData data, SnapshotData infos)
        {
            return Global.GetRepresentation(data, infos);
        }

        /// <summary>
        /// Gets the number of variables.
        /// </summary>
        /// <returns>Number of variables</returns>
        internal int GetNumberOfVariables()
        {
            int variables = 0;
            foreach (IndexContainer container in stack)
            {
                variables += container.Indexes.Count;
            }
            return variables;
        }
    }
}