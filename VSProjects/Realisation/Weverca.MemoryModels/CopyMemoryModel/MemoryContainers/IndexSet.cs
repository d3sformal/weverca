using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Represents cloneable and enumerable set of indexes which can be used in memory stack object.
    /// </summary>
    /// <typeparam name="T">Type of index object</typeparam>
    public class IndexSet<T> : IGenericCloneable<IndexSet<T>>, IEnumerable<T>
    {
        /// <summary>
        /// The set of used indexes
        /// </summary>
        private HashSet<T> indexes;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexSet{T}"/> class.
        /// </summary>
        public IndexSet()
        {
            indexes = new HashSet<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexSet{T}"/> class and fills set by given values.
        /// </summary>
        /// <param name="values">The values.</param>
        public IndexSet(IEnumerable<T> values)
        {
            indexes = new HashSet<T>(values);
        }

        /// <summary>
        /// Adds the specified index into set.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Add(T index)
        {
            indexes.Add(index);
        }
        /// <summary>
        /// Removes the specified index from set.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Remove(T index)
        {
            indexes.Remove(index);
        }
        /// <summary>
        /// Determines whether set contains specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool Contains(T index)
        {
            return indexes.Contains(index);
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public IndexSet<T> Clone()
        {
            return new IndexSet<T>(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return indexes.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return indexes.GetEnumerator();
        }
    }
}
