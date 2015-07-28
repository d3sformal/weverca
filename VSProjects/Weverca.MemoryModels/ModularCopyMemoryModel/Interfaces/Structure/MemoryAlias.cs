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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Instances of this factory class are used to create the new empty object
    /// which implements IMemoryAlias.
    /// </summary>
    public interface IMemoryAliasFactory
    {
        /// <summary>
        /// Creates the new instance of memory alias object to store alias definition in this structure.
        /// </summary>
        /// <param name="index">The memory index collection is created for.</param>
        /// <returns>Created alias collection.</returns>
        IMemoryAlias CreateMemoryAlias(IWriteableSnapshotStructure targetStructure, MemoryIndex index);
    }

    /// <summary>
    /// Contains information about alias structure for given memory location
    /// 
    /// Imutable class 
    ///     For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public interface IMemoryAlias
    {
        /// <summary>
        /// Gets the memory index which the aliases is binded to.
        /// </summary>
        /// <value>
        /// The memory index which the aliases is binded to.
        /// </value>
        MemoryIndex SourceIndex { get; }

        /// <summary>
        /// Gets the information whether this object contains some aliases.
        /// </summary>
        /// <value>
        /// The information whether this object contains some aliases.
        /// </value>
        bool HasAliases { get; }

        /// <summary>
        /// Gets the information whether this object contains some must aliases.
        /// </summary>
        /// <value>
        /// The information whether this object contains some must aliases.
        /// </value>
        bool HasMustAliases { get; }

        /// <summary>
        /// Gets the collection of may aliases.
        /// </summary>
        /// <value>
        /// The collection of may aliases.
        /// </value>
        IReadonlySet<MemoryIndex> MayAliases { get; }

        /// <summary>
        /// Gets the collection of must aliases.
        /// </summary>
        /// <value>
        /// The collection of must aliases.
        /// </value>
        IReadonlySet<MemoryIndex> MustAliases { get; }

        /// <summary>
        /// Creates new builder to modify this object
        /// </summary>
        /// <param name="targetStructure">The structure object for which a builder created.</param>
        /// <returns>
        /// New builder to modify this object.
        /// </returns>
        IMemoryAliasBuilder Builder(IWriteableSnapshotStructure targetStructure);
    }

    /// <summary>
    /// Mutable variant of MemoryAlias - use for creating new structure
    /// </summary>
    public interface IMemoryAliasBuilder
    {
        /// <summary>
        /// Gets the memory index which the aliases is binded to.
        /// </summary>
        /// <value>
        /// The memory index which the aliases is binded to.
        /// </value>
        MemoryIndex SourceIndex { get; }

        /// <summary>
        /// Gets the collection of may aliases.
        /// </summary>
        /// <value>
        /// The collection of may aliases.
        /// </value>
        IWriteableSet<MemoryIndex> MayAliases { get; }

        /// <summary>
        /// Gets the collection of must aliases.
        /// </summary>
        /// <value>
        /// The collection of must aliases.
        /// </value>
        IWriteableSet<MemoryIndex> MustAliases { get; }

        /// <summary>
        /// Sets the memory index which the aliases is binded to.
        /// </summary>
        /// <param name="index">The index.</param>
        void SetSourceIndex(MemoryIndex index);

        /// <summary>
        /// Builds new info object from this instance.
        /// </summary>
        /// <param name="targetStructure">The structure object for which is the instance built.</param>
        /// <returns>
        /// New imutable instance with data from this builder.
        /// </returns>
        IMemoryAlias Build(IWriteableSnapshotStructure targetStructure);
    }
}