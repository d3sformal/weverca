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
    /// Contains structural data about memory indexes. Every memory index used in snapshot is mapped
    /// to one instance of IndexData class. This class allows to set structural data like aliases,
    /// array associated with index and associated object which is used to traverse the memory tree.
    /// This class is extension of memory index so when there is structural change the instane of
    /// iIndexData is changed and memory index can stay the same which prevents cascade of changes
    /// across whole snapshot.
    /// 
    /// Imutable class. For modification use builder object 
    ///     data.Builder().modify().Build()
    /// </summary>
    public class IndexData
    {
        /// <summary>
        /// Gets the object with informations about alias structure for associated memory index.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        public MemoryAlias Aliases { get; private set; }

        /// <summary>
        /// Gets the array which is assocoated with memory index.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
        public AssociativeArray Array { get; private set; }

        /// <summary>
        /// Gets the list of all objects which are assocoated with memory index.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        public ObjectValueContainer Objects { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexData"/> class.
        /// </summary>
        /// <param name="aliases">The aliases.</param>
        /// <param name="array">The array.</param>
        /// <param name="objects">The objects.</param>
        public IndexData(MemoryAlias aliases, AssociativeArray array, ObjectValueContainer objects)
        {
            Aliases = aliases;
            Array = array;
            Objects = objects;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexData"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public IndexData(IndexDataBuilder data)
        {
            Aliases = data.Aliases;
            Array = data.Array;
            Objects = data.Objects;
        }

        /// <summary>
        /// Gets builder instance which can be used to modify this instance.
        /// </summary>
        /// <returns>New builder instance which can be used to modify this instance.</returns>
        public IndexDataBuilder Builder()
        {
            return new IndexDataBuilder(this);
        }

        /// <summary>
        /// Compares this data structure with the given object.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>True whether this instance contains the same values as the given one.</returns>
        internal bool DataEquals(IndexData other)
        {
            if (this == other)
            {
                return true;
            }

            if (this.Aliases != other.Aliases)
            {
                if (!MemoryAlias.AreEqual(Aliases, other.Aliases))
                {
                    return false;
                }
            }

            if (this.Array != other.Array)
            {
                if (Array == null || other.Array == null || !this.Array.Equals(other.Array))
                {
                    return false;
                }
            }

            if (this.Objects != other.Objects)
            {
                if (!ObjectValueContainer.AreEqual(Objects, other.Objects))
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Builder class to modify IndexData instances.
    /// </summary>
    public class IndexDataBuilder
    {
        /// <summary>
        /// Gets the object with informations about alias structure for associated memory index.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        public MemoryAlias Aliases { get; set; }

        /// <summary>
        /// Gets the array which is assocoated with memory index.
        /// </summary>
        /// <value>
        /// The array.
        /// </value>
        public AssociativeArray Array { get; set; }

        /// <summary>
        /// Gets the list of all objects which are assocoated with memory index.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        public ObjectValueContainer Objects { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexDataBuilder"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public IndexDataBuilder(IndexData data)
        {
            Aliases = data.Aliases;
            Array = data.Array;
            Objects = data.Objects;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns>New imutable container with data from this builder.</returns>
        public IndexData Build()
        {
            return new IndexData(this);
        }
    }
}