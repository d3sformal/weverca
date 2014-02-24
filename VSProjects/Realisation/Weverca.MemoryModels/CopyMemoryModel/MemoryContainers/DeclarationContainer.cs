﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Container for class and function declarations. Provides mapping between qualified 
    /// names and declarations data.
    /// 
    /// This is not imutable class.
    /// </summary>
    /// <typeparam name="T">Type of delared object.</typeparam>
    public class DeclarationContainer<T>
    {
        /// <summary>
        /// The collection of declarrations.
        /// </summary>
        private Dictionary<QualifiedName, HashSet<T>> declarations;

        /// <summary>
        /// Gets the count of declarations in the collection.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get { return declarations.Count; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarationContainer{T}"/> class.
        /// </summary>
        public DeclarationContainer()
        {
            declarations = new Dictionary<QualifiedName, HashSet<T>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeclarationContainer{T}"/> class and copy data from the givenb one.
        /// </summary>
        /// <param name="container">The container.</param>
        public DeclarationContainer(DeclarationContainer<T> container)
        {
            declarations = new Dictionary<QualifiedName, HashSet<T>>();
            foreach (var decl in container.declarations)
            {
                declarations[decl.Key] = new HashSet<T>(decl.Value);
            }
        }

        /// <summary>
        /// Determines whether the collection contains specified qualified name.
        /// </summary>
        /// <param name="key">The key qualified name.</param>
        /// <returns></returns>
        public bool ContainsKey(QualifiedName key)
        {
            return declarations.ContainsKey(key);
        }

        /// <summary>
        /// Tries to get the declaration by given qualified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(QualifiedName key, out IEnumerable<T> value)
        {
            HashSet<T> val;
            bool ret = declarations.TryGetValue(key, out val);

            value = val;
            return ret;
        }

        /// <summary>
        /// Gets the declaration by given qualified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public IEnumerable<T> GetValue(QualifiedName key)
        {
            return declarations[key];
        }

        /// <summary>
        /// Adds new declaration into the structure. If there
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(QualifiedName key, T value)
        {
            HashSet<T> set;
            if (!declarations.TryGetValue(key, out set))
            {
                set = new HashSet<T>();
                declarations[key] = set;
            }

            if (!set.Contains(value))
            {
                set.Add(value);
            }
        }

        /// <summary>
        /// Gets all qualified names which are defined in the container.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<QualifiedName> GetNames()
        {
            return declarations.Keys;
        }

        /// <summary>
        /// Determines whether this container contains the same definitions as the given one.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        internal bool DataEquals(DeclarationContainer<T> other)
        {
            HashSet<QualifiedName> names = new HashSet<QualifiedName>();
            HashSetTools.AddAll(names, this.declarations.Keys);
            HashSetTools.AddAll(names, other.declarations.Keys);

            foreach (var name in names)
            {
                HashSet<T> otherDecl, thisDecl;
                if (!other.declarations.TryGetValue(name, out otherDecl))
                {
                    return false;
                }

                if (!this.declarations.TryGetValue(name, out thisDecl))
                {
                    return false;
                }

                if (!HashSetTools.EqualsSet(thisDecl, otherDecl))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
