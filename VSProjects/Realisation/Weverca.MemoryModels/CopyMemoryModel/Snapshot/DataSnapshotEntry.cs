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

using Weverca.AnalysisFramework;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Implements snapshot entry functionality in order to manipulate with stored memory entries in copy memory model.
    /// 
    /// Memory entry is stored just in the Snaphot Entry instance where can be accesed with Snaphot Entry public interface.
    /// Any change on data persists memory entry as temporary memory in current context.
    /// </summary>
    public class DataSnapshotEntry : ReadWriteSnapshotEntryBase, ICopyModelSnapshotEntry
    {
        /// <summary>
        /// The data entry
        /// </summary>
        MemoryEntry dataEntry;

        /// <summary>
        /// The information entry
        /// </summary>
        MemoryEntry infoEntry;

        /// <summary>
        /// The temporary location
        /// </summary>
        SnapshotEntry temporaryLocation = null;

        /// <summary>
        /// The temporary index
        /// </summary>
        TemporaryIndex temporaryIndex = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSnapshotEntry"/> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="entry">The entry.</param>
        /// <exception cref="System.NotSupportedException">Current mode:  + snapshot.CurrentMode</exception>
        public DataSnapshotEntry(Snapshot snapshot, MemoryEntry entry)
        {
            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    dataEntry = entry;
                    infoEntry = new MemoryEntry();
                    break;

                case SnapshotMode.InfoLevel:
                    dataEntry = new MemoryEntry();
                    infoEntry = entry;
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (temporaryIndex != null)
            {
                return "temporary data: " + temporaryIndex.ToString();
            }
            else
            {
                return "temporary data: " + dataEntry.ToString();
            }
        }

        /// <summary>
        /// Gets the snapshot entry of temporary index associated with this memory entry.
        /// </summary>
        /// <param name="context">The context.</param>
        private SnapshotEntry getTemporary(SnapshotBase context)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);

            if (temporaryLocation == null)
            {
                temporaryIndex = snapshot.CreateTemporary();
                MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
                mergeWorker.MergeMemoryEntry(temporaryIndex, dataEntry);

                temporaryLocation = new SnapshotEntry(MemoryPath.MakePathTemporary(temporaryIndex));
            }

            return temporaryLocation;
        }

        /// <summary>
        /// Determines whether there is associated temporary index with memory entry in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>True whether there is associated temporary index with memory entry in the specified context.</returns>
        private bool isTemporarySet(SnapshotBase context)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);

            return temporaryIndex != null && snapshot.IsTemporarySet(temporaryIndex);
        }

        #region ReadWriteSnapshotEntryBase Implementation

        #region Navigation

        /// <summary>
        /// Read memory represented by given index identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="index">Identifier of an index</param>
        /// <returns>
        /// Snapshot entry representing index resolving on current entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index)
        {
            SnapshotLogger.append(context, "read index - " + this.ToString());

            return getTemporary(context).ReadIndex(context, index);
        }

        /// <summary>
        /// Read memory represented by given field identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="field">Identifier of an field</param>
        /// <returns>
        /// Snapshot entry representing field resolving on current entry
        /// </returns>
        protected override ReadWriteSnapshotEntryBase readField(SnapshotBase context, AnalysisFramework.VariableIdentifier field)
        {
            SnapshotLogger.append(context, "read index - " + this.ToString());

            return getTemporary(context).ReadField(context, field);
        }

        #endregion

        #region Update

        /// <summary>
        /// Write given value at memory represented by snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="value">Written value</param>
        /// <param name="forceStrongWrite">Determine that current write should be processed as strong</param>
        /// <exception cref="System.NotSupportedException">Current mode:  + snapshot.CurrentMode</exception>
        protected override void writeMemory(SnapshotBase context, MemoryEntry value, bool forceStrongWrite)
        {
            SnapshotLogger.append(context, "write memory - " + this.ToString());
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);

            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    getTemporary(context).WriteMemory(context, value, forceStrongWrite);
                    break;

                case SnapshotMode.InfoLevel:
                    if (isTemporarySet(context))
                    {
                        getTemporary(context).WriteMemory(context, value, forceStrongWrite);
                    }
                    else
                    {
                        infoEntry = value;
                    }
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <summary>
        /// Set aliases to current snapshot entry. Aliases can be set even to those entries
        /// that doesn't belongs to any variable, field,..
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="aliasedEntry">Snapshot entry which will be aliased from current entry</param>
        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            SnapshotLogger.append(context, "set aliases - " + this.ToString());

            getTemporary(context).SetAliases(context, aliasedEntry);
        }

        #endregion

        #region Read

        /// <summary>
        /// Determine that memory represented by current snapshot entry Is already defined.
        /// If not, reading memory returns UndefinedValue. But UndefinedValue can be returned
        /// even for defined memory entries - this can be used to distinct
        /// between null/undefined semantic of PHP.
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>True whether that memory represented by current snapshot entry Is already defined.</returns>
        protected override bool isDefined(SnapshotBase context)
        {
            SnapshotLogger.append(context, "is defined - " + this.ToString());

            if (isTemporarySet(context))
            {
                return temporaryLocation.IsDefined(context);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns aliases that can be used for making alias join
        /// to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>
        /// Aliases of current snapshot entry
        /// </returns>
        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            SnapshotLogger.append(context, "aliases - " + this.ToString());

            if (isTemporarySet(context))
            {
                return temporaryLocation.Aliases(context);
            }
            else
            {
                return new AliasEntry[] { };
            }
        }

        /// <summary>
        /// Read memory represented by current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>
        /// Memory represented by current snapshot entry
        /// </returns>
        /// <exception cref="System.NotSupportedException">Current mode:  + snapshot.CurrentMode</exception>
        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            SnapshotLogger.append(context, "read memory - " + this.ToString());

            if (isTemporarySet(context))
            {
                SnapshotLogger.append(context, "read from temporary location - " + this.ToString());
                return temporaryLocation.ReadMemory(context);
            }
            else
            {
                SnapshotLogger.append(context, "read just value - " + this.ToString());
                Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
                switch (snapshot.CurrentMode)
                {
                    case SnapshotMode.MemoryLevel:
                        return dataEntry;

                    case SnapshotMode.InfoLevel:
                        return infoEntry;

                    default:
                        throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
                }
            }
        }

        /// <summary>
        /// Resolve method on current snapshot entry with given methodName
        /// </summary>
        /// <param name="context">Context where methods are resolved</param>
        /// <param name="methodName">Name of resolved method</param>
        /// <returns>
        /// Resolved methods
        /// </returns>
        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, PHP.Core.QualifiedName methodName)
        {
            SnapshotLogger.append(context, "resolve method - " + this.ToString() + " method: " + methodName);

            if (isTemporarySet(context))
            {
                return temporaryLocation.ResolveMethod(context, methodName);
            }
            else
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
                return snapshot.resolveMethod(dataEntry, methodName);
            }
        }

        /// <summary>
        /// Returns variables corresponding to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>
        /// Variable identifier of current snapshot entry or null if entry doesn't belong to variable
        /// </returns>
        /// <exception cref="System.Exception">No variable identifier can be get for memory entry snapshot.</exception>
        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            throw new Exception("No variable identifier can be get for memory entry snapshot.");
        }

        #endregion

        #endregion

        /// <summary>
        /// Creates the alias to this entry and returnes data which can be used to aliasing the target.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Alias data fro the newly created aliases.</returns>
        public AliasData CreateAliasToEntry(Snapshot snapshot)
        {
            return getTemporary(snapshot).CreateAliasToEntry(snapshot);
        }

        /// <summary>
        /// Write given value at memory represented by snapshot entry and doesn't process any
        /// array copy. Is needed for correct increment/decrement semantic.
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="value">Written value</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            if (isTemporarySet(context))
            {
                getTemporary(context).WriteMemoryWithoutCopy(context, value);
            }
            else
            {
                Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
                switch (snapshot.CurrentMode)
                {
                    case SnapshotMode.MemoryLevel:
                        dataEntry = value;
                        break;

                    case SnapshotMode.InfoLevel:
                        infoEntry = value;
                        break;

                    default:
                        throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
                }
            }
        }

        /// <summary>
        /// Iterate fields defined on object
        /// </summary>
        /// <param name="context">Context where fields are searched</param>
        /// <returns>
        /// Enumeration of available fields
        /// </returns>
        protected override IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context)
        {
            return SnapshotEntryHelper.IterateFields(context, this);
        }

        /// <summary>
        /// Iterate indexes defined on array
        /// </summary>
        /// <param name="context">Context where indexes are searched</param>
        /// <returns>
        /// Enumeration of available fields
        /// </returns>
        protected override IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context)
        {
            return SnapshotEntryHelper.IterateIndexes(context, this);
        }

        /// <summary>
        /// Resolve type of objects in snapshot entry
        /// </summary>
        /// <param name="context">Context where types are resolved</param>
        /// <returns>
        /// Resolved types
        /// </returns>
        protected override IEnumerable<TypeValue> resolveType(SnapshotBase context)
        {
            return SnapshotEntryHelper.ResolveType(context, this);
        }

        /// <summary>
        /// Calls snapshot entry interface method readMemory to provide standard read from snapshot entry.
        /// </summary>
        /// <param name="snapshot">The memory context.</param>
        /// <returns>
        /// Memory represented by current snapshot entry.
        /// </returns>
        public MemoryEntry ReadMemory(Snapshot snapshot)
        {
            return this.readMemory(snapshot);
        }
    }
}