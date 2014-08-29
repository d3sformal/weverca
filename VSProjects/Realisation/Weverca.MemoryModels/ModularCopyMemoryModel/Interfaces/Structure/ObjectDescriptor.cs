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
    /// Stores type and list of fields of PHP object. Every objects is determined by special value object
    /// with no content. This object is just pointer which is used to receive this descriptor instance when is need.
    /// This approach allows to use pointer object across whole memory model without need of modyfying structure
    /// when some new idnex is added - snapshot just simply modify descriptor instance and let the pointer to be
    /// still the same.
    /// 
    /// Imutable class. For modification use builder object 
    ///         descriptor.Builder().modify().Build() //Creates new modified object
    /// </summary>
    public interface IObjectDescriptor : IReadonlyIndexContainer
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        TypeValue Type { get; }

        /// <summary>
        /// Gets the object value representation of this object.
        /// </summary>
        /// <value>
        /// The object value.
        /// </value>
        ObjectValue ObjectValue { get; }

        /// <summary>
        /// Gets container builder to create new imutable instance with modified data.
        /// </summary>
        /// <returns>New builder to modify this descriptor.</returns>
        IObjectDescriptorBuilder Builder();
    }

    /// <summary>
    /// Mutable variant of ObjectDescriptor - use for creating new structure
    /// </summary>
    public interface IObjectDescriptorBuilder : IWriteableIndexContainer
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        TypeValue Type { get; }

        /// <summary>
        /// Gets the object value representation of this object.
        /// </summary>
        /// <value>
        /// The object value.
        /// </value>
        ObjectValue ObjectValue { get; }

        /// <summary>
        /// Sets the type.
        /// </summary>
        /// <param name="type">The type.</param>
        void SetType(TypeValue type);

        /// <summary>
        /// Sets the objectvalue.
        /// </summary>
        /// <param name="objectValue">The object value.</param>
        void SetObjectValue(ObjectValue objectValue);

        /// <summary>
        /// Gets the imutable version of this collection.
        /// </summary>
        /// <returns>The imutable version of this collection.</returns>
        IObjectDescriptor Build();
    }
}