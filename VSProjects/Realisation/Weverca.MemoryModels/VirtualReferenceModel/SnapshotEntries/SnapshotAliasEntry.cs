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

using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.VirtualReferenceModel.SnapshotEntries
{
    /// <summary>
    /// Alias on <see cref="SnapshotMemoryEntry"/>
    /// </summary>
    class SnapshotAliasEntry : AliasEntry
    {
        /// <summary>
        /// Aliased entry
        /// </summary>
        internal readonly SnapshotMemoryEntry SnapshotEntry;

        internal SnapshotAliasEntry(SnapshotMemoryEntry entry)
        {
            SnapshotEntry = entry;
        }
    }
}