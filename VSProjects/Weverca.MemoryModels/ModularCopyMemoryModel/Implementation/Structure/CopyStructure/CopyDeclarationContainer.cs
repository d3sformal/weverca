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


using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Utils;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Structure.CopyStructure
{
    class CopyDeclarationContainerFactory : IDeclarationContainerFactory
    {
        public IWriteableDeclarationContainer<T> CreateWriteableDeclarationContainer<T>()
        {
            return new CopyDeclarationContainer<T>();
        }
    }

    /// <summary>
    /// Container for class and function declarations. Provides mapping between qualified 
    /// names and declarations data.
    /// 
    /// This is not imutable class.
    /// </summary>
    /// <typeparam name="T">Type of delared object.</typeparam>
    class CopyDeclarationContainer<T> : IWriteableDeclarationContainer<T>
    {
        /// <summary>
        /// The collection of declarrations.
        /// </summary>
        private Dictionary<QualifiedName, CopySet<T>> declarations;

        /// <summary>
        /// Gets the count of declarations in the collection.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get { return declarations.Count; } }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDeclarationContainer{T}"/> class.
        /// </summary>
        public CopyDeclarationContainer()
        {
            declarations = new Dictionary<QualifiedName, CopySet<T>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyDeclarationContainer{T}"/> class and copy data from the given one.
        /// </summary>
        /// <param name="container">The container.</param>
        public CopyDeclarationContainer(CopyDeclarationContainer<T> container)
        {
            declarations = new Dictionary<QualifiedName, CopySet<T>>();
            foreach (var decl in container.declarations)
            {
                declarations[decl.Key] = new CopySet<T>(decl.Value);
            }
        }

        /// <summary>
        /// Determines whether the collection contains specified qualified name.
        /// </summary>
        /// <param name="key">The key qualified name.</param>
        /// <returns>True whether the container contains declaration with the given name.</returns>
        public bool Contains(QualifiedName key)
        {
            return declarations.ContainsKey(key);
        }

        /// <summary>
        /// Tries to get the declaration by given qualified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True whether the container contains declaration with the given name.</returns>
        public bool TryGetValue(QualifiedName key, out IEnumerable<T> value)
        {
            CopySet<T> val;
            bool ret = declarations.TryGetValue(key, out val);

            value = val;
            return ret;
        }

        /// <summary>
        /// Gets the declaration by given qualified name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Declaration with the given name.</returns>
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
            CopySet<T> set;
            if (!declarations.TryGetValue(key, out set))
            {
                set = new CopySet<T>();
                declarations[key] = set;
            }

            if (!set.Contains(value))
            {
                set.Add(value);
            }
        }

        /// <summary>
        /// Sets all given declaration for declarations with given name.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public void SetAll(QualifiedName key, IEnumerable<T> values)
        {
            CopySet<T> set;
            if (!declarations.TryGetValue(key, out set))
            {
                set = new CopySet<T>();
                declarations[key] = set;
            }

            set.Clear();
            set.AddAll(values);
        }

        /// <summary>
        /// Gets all qualified names which are defined in the container.
        /// </summary>
        /// <returns>List of names of all declarations in this container.</returns>
        public IEnumerable<QualifiedName> GetNames()
        {
            return declarations.Keys;
        }

        public IWriteableDeclarationContainer<T> Copy()
        {
            return new CopyDeclarationContainer<T>(this);
        }
    }
}