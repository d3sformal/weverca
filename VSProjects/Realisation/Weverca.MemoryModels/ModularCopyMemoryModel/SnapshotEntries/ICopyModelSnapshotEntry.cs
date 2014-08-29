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
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries
{
    /// <summary>
    /// Defines basic methods for copy memory model snapshot entries.
    /// </summary>
    public interface ICopyModelSnapshotEntry
    {
        /// <summary>
        /// Gets the path of this snapshot entry.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>The path of this snapshot entry.</returns>
        MemoryPath GetPath(Snapshot snapshot);

        /// <summary>
        /// Reads the memory.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Memory represented by current snapshot entry.</returns>
        MemoryEntry ReadMemory(Snapshot snapshot);
    }
}