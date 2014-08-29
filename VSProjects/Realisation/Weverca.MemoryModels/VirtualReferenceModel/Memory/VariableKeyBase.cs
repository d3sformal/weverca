/*
Copyright (c) 2012-2014 Miroslav Vodolan.

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

namespace Weverca.MemoryModels.VirtualReferenceModel.Memory
{
    /// <summary>
    /// Base class for keys of variables stored within <see cref="Snapshot"/>
    /// </summary>
    abstract class VariableKeyBase
    {
        /// <summary>
        /// Get or create variable  in context of given snapshot
        /// </summary>
        /// <param name="snapshot">Context snapshot</param>
        /// <returns>Created or obtained variable info</returns>
        internal abstract VariableInfo GetOrCreateVariable(Snapshot snapshot);

        /// <summary>
        /// Get variable in context of given snapshot
        /// </summary>
        /// <param name="snapshot">Context snapshot</param>
        /// <returns>Obtained variable info</returns>
        internal abstract VariableInfo GetVariable(Snapshot snapshot);

        /// <summary>
        /// Create implicit reference of belonging variable in context of given snapshot
        /// </summary>
        /// <param name="snapshot">Context snapshot</param>
        /// <returns>Created or obtained variable info</returns>
        internal abstract VirtualReference CreateImplicitReference(Snapshot snapshot);
    }
}