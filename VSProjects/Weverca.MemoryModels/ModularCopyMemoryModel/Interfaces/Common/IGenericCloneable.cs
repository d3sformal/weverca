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

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Common
{
    /// <summary>
    /// Generic version of cloneable interface.
    /// </summary>
    /// <typeparam name="T">Typpe of class which shoul be cloned</typeparam>
    public interface IGenericCloneable<T>
    {
        /// <summary>
        /// Creates deep copy of this instance.
        /// </summary>
        /// <returns>New instance which contains copy of this instance.</returns>
        T Clone();
    }
}