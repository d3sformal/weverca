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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Contains information about alias structure for given memory location
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public class MemoryAlias
    {
        /// <summary>
        /// Gets the may aliasses.
        /// </summary>
        /// <value>
        /// The may aliasses.
        /// </value>
        public ReadOnlyCollection<MemoryIndex> MayAliasses { get; private set; }

        /// <summary>
        /// Gets the must aliasses.
        /// </summary>
        /// <value>
        /// The must aliasses.
        /// </value>
        public ReadOnlyCollection<MemoryIndex> MustAliasses { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAlias"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        internal MemoryAlias(MemoryIndex index)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[] { });
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(new MemoryIndex[] { index });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAlias"/> class from the builder object.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal MemoryAlias(MemoryAliasBuilder builder)
        {
            MayAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MayAliasses.ToList());
            MustAliasses = new ReadOnlyCollection<MemoryIndex>(builder.MustAliasses.ToList());
        }

        /// <summary>
        /// Creates new builder to modify this object 
        /// </summary>
        /// <returns>New builder to modify this object.</returns>
        public MemoryAliasBuilder Builder()
        {
            return new MemoryAliasBuilder(this);
        }
        
        internal void ToString(StringBuilder builder)
        {
            if (MustAliasses.Count > 0)
            {
                builder.Append("\n MUST ALIASES: ");
                foreach (MemoryIndex alias in MustAliasses)
                {
                    builder.Append(alias.ToString());
                    builder.Append(" | ");
                }
            }

            if (MayAliasses.Count > 0)
            {
                builder.Append("\n MAY ALIASES: ");
                foreach (MemoryIndex alias in MayAliasses)
                {
                    builder.Append(alias.ToString());
                    builder.Append(" | ");
                }
            }
        }

        internal bool DataEquals(MemoryAlias other)
        {
            if (other == null)
            {
                return this.MayAliasses.Count == 0 && this.MustAliasses.Count == 0;
            }

            if (this.MayAliasses.Count != other.MayAliasses.Count
                || this.MustAliasses.Count != other.MustAliasses.Count)
            {
                return false;
            }

            return HashSetTools.EqualsSet(this.MustAliasses, other.MustAliasses) 
                || HashSetTools.EqualsSet(this.MayAliasses, other.MayAliasses);
        }

        internal static bool AreEqual(MemoryAlias objA, MemoryAlias objB)
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
    /// Mutable variant of MemoryAlias - use for creating new structure
    /// </summary>
    public class MemoryAliasBuilder
    {
        /// <summary>
        /// Gets the may aliasses.
        /// </summary>
        /// <value>
        /// The may aliasses.
        /// </value>
        public HashSet<MemoryIndex> MayAliasses { get; private set; }

        /// <summary>
        /// Gets the must aliasses.
        /// </summary>
        /// <value>
        /// The must aliasses.
        /// </value>
        public HashSet<MemoryIndex> MustAliasses { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAliasBuilder"/> class.
        /// </summary>
        /// <param name="MemoryAlias">The memory information.</param>
        public MemoryAliasBuilder(MemoryAlias MemoryAlias)
        {
            MayAliasses = new HashSet<MemoryIndex>(MemoryAlias.MayAliasses);
            MustAliasses = new HashSet<MemoryIndex>(MemoryAlias.MustAliasses);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAliasBuilder"/> class.
        /// </summary>
        public MemoryAliasBuilder()
        {
            MayAliasses = new HashSet<MemoryIndex>();
            MustAliasses = new HashSet<MemoryIndex>();
        }

        /// <summary>
        /// Builds new info object from this instance.
        /// </summary>
        /// <returns>New imutable instance with data from this builder.</returns>
        public MemoryAlias Build()
        {
            return new MemoryAlias(this);
        }

        /// <summary>
        /// Removes the must alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>This builder instance.</returns>
        public MemoryAliasBuilder RemoveMustAlias(MemoryIndex index)
        {
            MustAliasses.Remove(index);
            return this;
        }

        /// <summary>
        /// Removes the may alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>This builder instance.</returns>
        public MemoryAliasBuilder RemoveMayAlias(MemoryIndex index)
        {
            MayAliasses.Remove(index);
            return this;
        }

        /// <summary>
        /// Adds the must alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>This builder instance.</returns>
        public MemoryAliasBuilder AddMustAlias(MemoryIndex index)
        {
            MustAliasses.Add(index);
            return this;
        }

        /// <summary>
        /// Adds the may alias.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>This builder instance.</returns>
        public MemoryAliasBuilder AddMayAlias(MemoryIndex index)
        {
            MayAliasses.Add(index);
            return this;
        }

        /// <summary>
        /// Adds the may alias.
        /// </summary>
        /// <param name="aliases">The aliases.</param>
        internal void AddMayAlias(IEnumerable<MemoryIndex> aliases)
        {
            HashSetTools.AddAll(MayAliasses, aliases);
        }

        /// <summary>
        /// Adds the must alias.
        /// </summary>
        /// <param name="aliases">The aliases.</param>
        internal void AddMustAlias(IEnumerable<MemoryIndex> aliases)
        {
            HashSetTools.AddAll(MustAliasses, aliases);
        }
    }
}