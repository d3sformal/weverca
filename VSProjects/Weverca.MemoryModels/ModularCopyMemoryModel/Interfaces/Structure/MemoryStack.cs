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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Represents single level of memory stack with collections of variables and defined arrays.
    /// This level is part of memory stack an allows to separate global and local memory contexts.
    /// 
    /// This interface contains readonly items.
    /// </summary>
    public interface IReadonlyStackContext
    {

        /// <summary>
        /// Gets the stack level.
        /// </summary>
        /// <value>
        /// The stacklevel.
        /// </value>
        int StackLevel { get; }

        /// <summary>
        /// Gets the readonly collection with associative container of definition of variables.
        /// </summary>
        /// <value>
        /// The readonly collection of variables variables.
        /// </value>
        IReadonlyIndexContainer ReadonlyVariables { get; }

        /// <summary>
        /// Gets the readonly collection with associative container of definition of controll variables.
        /// </summary>
        /// <value>
        /// The readonly collection of controll variables.
        /// </value>
        IReadonlyIndexContainer ReadonlyControllVariables { get; }

        /// <summary>
        /// Gets the readonly set of temporary variables.
        /// </summary>
        /// <value>
        /// The readoly set of temporary variables.
        /// </value>
        IReadonlySet<MemoryIndex> ReadonlyTemporaryVariables { get; }

        /// <summary>
        /// Gets the readonly set of definitions of arrays.
        /// </summary>
        /// <value>
        /// The readonly set of definitions of arrays.
        /// </value>
        IReadonlySet<AssociativeArray> ReadonlyArrays { get; }
    }

    /// <summary>
    /// Represents single level of memory stack with collections of variables and defined arrays.
    /// This level is part of memory stack an allows to separate global and local memory contexts.
    /// 
    /// This interface allows modification of inner structure.
    /// </summary>
    public interface IWriteableStackContext : IReadonlyStackContext, IGenericCloneable<IWriteableStackContext>
    {
        /// <summary>
        /// Gets the writeable collection with associative container of definition of variables.
        /// </summary>
        /// <value>
        /// The writeable collection of variables variables.
        /// </value>
        IWriteableIndexContainer WriteableVariables { get; }

        /// <summary>
        /// Gets the writeable collection with associative container of definition of controll variables.
        /// </summary>
        /// <value>
        /// The writeable collection of controll variables.
        /// </value>
        IWriteableIndexContainer WriteableControllVariables { get; }

        /// <summary>
        /// Gets the readoly set of temporary variables.
        /// </summary>
        /// <value>
        /// The readoly set of temporary variables.
        /// </value>
        IWriteableSet<MemoryIndex> WriteableTemporaryVariables { get; }

        /// <summary>
        /// Gets the writeable set of definitions of arrays.
        /// </summary>
        /// <value>
        /// The writeable set of definitions of arrays.
        /// </value>
        IWriteableSet<AssociativeArray> WriteableArrays { get; }
    }
}