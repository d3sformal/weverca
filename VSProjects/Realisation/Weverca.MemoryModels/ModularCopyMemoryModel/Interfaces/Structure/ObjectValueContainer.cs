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
    /// Contains all PHP objects which can be stored in some memory location. This class is used as part
    /// of IndexData objects to provide shortcut in order to not to access memory entry for the list
    /// of objects. Algorithms can easilly found that there is no object or even list all objects without
    /// lookup in data and listing all values in memory entry.
    /// 
    /// Imutable class. For modification use builder object 
    ///     data.Builder().modify().Build()
    /// </summary>
    public interface IObjectValueContainer : IReadonlySet<ObjectValue>
    {
        /// <summary>
        /// Gets container builder to create new imutable instance with modified data.
        /// </summary>
        /// <returns>New builder to modify this descriptor.</returns>
        IObjectValueContainerBuilder Builder();
    }

    /// <summary>
    /// Builder class to modify ObjectValueContainer instances.
    /// </summary>
    public interface IObjectValueContainerBuilder : IWriteableSet<ObjectValue>
    {
        /// <summary>
        /// Gets the imutable version of this collection.
        /// </summary>
        /// <returns>The imutable version of this collection.</returns>
        IObjectValueContainer Build();
    }
}