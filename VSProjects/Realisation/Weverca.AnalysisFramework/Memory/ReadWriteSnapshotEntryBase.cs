/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represents entry provided by snapshots. Provides accessing to memory based operations that CAN MODIFY
    /// visible state of snapshot (read write operation abstraction)
    /// </summary>
    public abstract class ReadWriteSnapshotEntryBase : ReadSnapshotEntryBase
    {
        /// <summary>
        /// Write given value at memory represented by snapshot entry
        /// </summary>
        /// <param name="value">Written value</param>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="forceStrongWrite">Determine that current write should be processed as strong</param>
        protected abstract void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite);

        /// <summary>
        /// Write given value at memory represented by snapshot entry and doesn't process any
        /// array copy. Is needed for correct increment/decrement semantic.
        /// </summary>
        /// <param name="value">Written value</param>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        protected abstract void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value);

        /// <summary>
        /// Set aliases to current snapshot entry. Aliases can be set even to those entries
        /// that doesn't belongs to any variable, field,..
        /// </summary>
        /// <param name="aliasedEntry">Snapshot entry which will be aliased from current entry</param>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        protected abstract void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry);

        /// <summary>
        /// Write given value at memory represented by snapshot entry
        /// </summary>
        /// <param name="value">Written value</param>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="forceStrongWrite">Determine that current write should be processed as strong</param>
        public void WriteMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite = false)
        {
            //TODO statistics reporting
            writeMemory(context, value, forceStrongWrite);
        }

        /// <summary>
        /// Write given value at memory represent by snasphot entry. No value copy operations can be proceeded
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="value">Written value</param>
        public void WriteMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            //TODO statistics reporting
            writeMemoryWithoutCopy(context, value);
        }

        /// <summary>
        /// Set aliases to current snapshot entry. Aliases can be set even to those entries
        /// that doesn't belongs to any variable, field,..
        /// </summary>
        /// <param name="aliasedEntry">Snapshot entry which will be aliased from current entry</param>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        public void SetAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            //TODO statistics reporting
            setAliases(context, aliasedEntry);
        }
    }
}