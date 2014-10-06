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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Contains structural data about memory indexes. Every memory index used in snapshot is mapped
    /// to one instance of IIndexDefinition interface. This interface allows to set structural data like aliases,
    /// array associated with index and associated object which is used to traverse the memory tree.
    /// This class is extension of memory index so when there is structural change the instane of
    /// iIndexData is changed and memory index can stay the same which prevents cascade of changes
    /// across whole snapshot.
    /// 
    /// Imutable class.
    /// </summary>
    public interface IIndexDefinition
    {
        /// <summary>
        /// Gets the object with informations about alias structure for associated memory index.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        IMemoryAlias Aliases { get; }

        /// <summary>
        /// Gets the array which is assocoated with memory index.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
        IObjectValueContainer Objects { get; }

        /// <summary>
        /// Gets the list of all objects which are assocoated with memory index.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        AssociativeArray Array { get; }

        /// <summary>
        /// Gets container builder to create new imutable instance with modified data.
        /// </summary>
        /// <returns>New builder to modify this descriptor.</returns>
        IIndexDefinitionBuilder Builder();
    }

    /// <summary>
    /// Contains structural data about memory indexes. Every memory index used in snapshot is mapped
    /// to one instance of IIndexDefinition interface. This interface allows to set structural data like aliases,
    /// array associated with index and associated object which is used to traverse the memory tree.
    /// This class is extension of memory index so when there is structural change the instane of
    /// iIndexData is changed and memory index can stay the same which prevents cascade of changes
    /// across whole snapshot.
    /// 
    /// Builder class.
    /// </summary>
    public interface IIndexDefinitionBuilder
    {
        /// <summary>
        /// Sets the array.
        /// </summary>
        /// <param name="arrayValue">The array value.</param>
        void SetArray(AssociativeArray arrayValue);

        /// <summary>
        /// Sets the objects.
        /// </summary>
        /// <param name="objects">The objects.</param>
        void SetObjects(IObjectValueContainer objects);

        /// <summary>
        /// Sets the aliases.
        /// </summary>
        /// <param name="aliases">The aliases.</param>
        void SetAliases(IMemoryAlias aliases);

        /// <summary>
        /// Gets the imutable version of this collection.
        /// </summary>
        /// <returns>The imutable version of this collection.</returns>
        IIndexDefinition Build();
    }
}