using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Contains all PHP objects which can be stored in some memory location. This class is used as part
    /// of IndexData objects to provide shortcut in order to not to access memory entry for the list
    /// of objects. Algorithms can easilly found that there is no object or even list all objects without
    /// lookup in data and listing all values in memory entry.
    /// 
    /// Imutable class. For modification use builder object 
    ///     data.Builder().modify().Build()
    /// </summary>
    public class ObjectValueContainer : IEnumerable<ObjectValue>
    {
        /// <summary>
        /// The object values
        /// </summary>
        HashSet<ObjectValue> values;

        /// <summary>
        /// Gets the number of objects in container.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get { return values.Count; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueContainer"/> class.
        /// </summary>
        public ObjectValueContainer()
        {
            values = new HashSet<ObjectValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueContainer"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public ObjectValueContainer(ObjectValueContainerBuilder builder)
        {
            values = new HashSet<ObjectValue>(builder.Values);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueContainer"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        public ObjectValueContainer(IEnumerable<ObjectValue> values)
        {
            this.values = new HashSet<ObjectValue>(values);
        }

        /// <summary>
        /// Determines whether container contains specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool Contains(ObjectValue value)
        {
            return values.Contains(value);
        }

        /// <summary>
        /// Gets the builder object to modify this container.
        /// </summary>
        /// <returns></returns>
        public ObjectValueContainerBuilder Builder()
        {
            return new ObjectValueContainerBuilder(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ObjectValue> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        /// <summary>
        /// Compares this data structure with the given object.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        internal bool DataEquals(ObjectValueContainer other)
        {
            if (other == null)
            {
                return this.values.Count == 0;
            }

            if (this.values.Count != other.values.Count)
            {
                return false;
            }

            foreach (ObjectValue value in values)
            {
                if (!other.values.Contains(value))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether container contains specified value.
        /// </summary>
        /// <param name="objA">The object aggregate.</param>
        /// <param name="objB">The object attribute.</param>
        /// <returns></returns>
        internal static bool AreEqual(ObjectValueContainer objA, ObjectValueContainer objB)
        {
            if (objA == objB)
            {
                return true;
            }

            if (objA != null)
            {
                return objA.DataEquals(objB);
            }
            else
            {
                return objB.DataEquals(objA);
            }
        }
    }

    /// <summary>
    /// Builder class to modify ObjectValueContainer instances.
    /// </summary>
    public class ObjectValueContainerBuilder : IEnumerable<ObjectValue>
    {
        /// <summary>
        /// Gets the object values.
        /// </summary>
        /// <value>
        /// The object values.
        /// </value>
        public HashSet<ObjectValue> Values { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectValueContainerBuilder"/> class.
        /// </summary>
        /// <param name="objectValueContainer">The object value container.</param>
        public ObjectValueContainerBuilder(ObjectValueContainer objectValueContainer)
        {
            Values = new HashSet<ObjectValue>(objectValueContainer);
        }

        /// <summary>
        /// Adds new object into collection.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(ObjectValue value)
        {
            Values.Add(value);
        }
        /// <summary>
        /// Removes object from collection.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Remove(ObjectValue value)
        {
            Values.Remove(value);
        }
        /// <summary>
        /// Determines whether collection contains the specified object.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool Contains(ObjectValue value)
        {
            return Values.Contains(value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            Values.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ObjectValue> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public ObjectValueContainer Build()
        {
            return new ObjectValueContainer(this);
        }
    }
}
