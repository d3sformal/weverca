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
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Defines basic methods for copy memory model snapshot entries.
    /// </summary>
    public interface ICopyModelSnapshotEntry
    {
        /// <summary>
        /// Creates the alias to this entry and returnes data which can be used to aliasing the target.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Alias data fro the newly created aliases.</returns>
        AliasData CreateAliasToEntry(Snapshot snapshot);

        /// <summary>
        /// Reads the memory.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>Memory represented by current snapshot entry.</returns>
        MemoryEntry ReadMemory(Snapshot snapshot);
    }

    /// <summary>
    /// Static class which contains common functionality for snapshot entry classes
    /// </summary>
    class SnapshotEntryHelper
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="SnapshotEntryHelper"/> class from being created.
        /// </summary>
        private SnapshotEntryHelper()
        {
        }

        /// <summary>
        /// Iterates the fields of PHP object in entry.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="snapshotEntry">The snapshot entry.</param>
        public static IEnumerable<VariableIdentifier> IterateFields(SnapshotBase context, ICopyModelSnapshotEntry snapshotEntry)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);

            return visitor.CollectFields(snapshot);
        }

        /// <summary>
        /// Iterates the indexes of PHP array in entry.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="snapshotEntry">The snapshot entry.</param>
        /// <returns>The list of indexes in the given snapshot entry.</returns>
        public static IEnumerable<MemberIdentifier> IterateIndexes(SnapshotBase context, ICopyModelSnapshotEntry snapshotEntry)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);

            return visitor.CollectIndexes(snapshot);
        }

        /// <summary>
        /// Resolves the type of PHP objects in the entry.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="snapshotEntry">The snapshot entry.</param>
        /// <returns>The list of types in the given snapshot entry.</returns>
        public static IEnumerable<TypeValue> ResolveType(SnapshotBase context, ICopyModelSnapshotEntry snapshotEntry)
        {
            Snapshot snapshot = SnapshotEntry.ToSnapshot(context);
            MemoryEntry entry = snapshotEntry.ReadMemory(snapshot);

            CollectComposedValuesVisitor visitor = new CollectComposedValuesVisitor();
            visitor.VisitMemoryEntry(entry);

            return visitor.ResolveObjectsTypes(snapshot);
        }
    }

    /// <summary>
    /// Collects information about aliases in order to transfer it between entries.
    /// </summary>
    public class AliasData
    {
        /// <summary>
        /// The temporary index
        /// </summary>
        private TemporaryIndex temporaryIndex;

        /// <summary>
        /// Gets the must indexes.
        /// </summary>
        /// <value>
        /// The must indexes.
        /// </value>
        public IEnumerable<MemoryIndex> MustIndexes { get; private set; }

        /// <summary>
        /// Gets the may indexes.
        /// </summary>
        /// <value>
        /// The may indexes.
        /// </value>
        public IEnumerable<MemoryIndex> MayIndexes { get; private set; }

        /// <summary>
        /// Gets the index of the source.
        /// </summary>
        /// <value>
        /// The index of the source.
        /// </value>
        public MemoryIndex SourceIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasData"/> class.
        /// </summary>
        /// <param name="mustIndexes">The must indexes.</param>
        /// <param name="mayIndexes">The may indexes.</param>
        /// <param name="sourceIndex">Index of the source of data.</param>
        public AliasData(IEnumerable<MemoryIndex> mustIndexes, IEnumerable<MemoryIndex> mayIndexes, MemoryIndex sourceIndex)
        {
            if (mustIndexes != null)
            {
                this.MustIndexes = mustIndexes;
            }
            else
            {
                this.MustIndexes = new MemoryIndex[] { };
            }

            if (mayIndexes != null)
            {
                this.MayIndexes = mayIndexes;
            }
            else
            {
                this.MayIndexes = new MemoryIndex[] { };
            }

            this.SourceIndex = sourceIndex;
        }

        /// <summary>
        /// Releases thestored temporary object in given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        internal void Release(Snapshot snapshot)
        {
            if (temporaryIndex != null)
            {
                snapshot.ReleaseTemporary(temporaryIndex);
            }
        }

        /// <summary>
        /// Sets the temporary index which can be released using Release method.
        /// </summary>
        /// <param name="temporaryIndex">Temporary index to realease.</param>
        internal void TemporaryIndexToRealease(TemporaryIndex temporaryIndex)
        {
            this.temporaryIndex = temporaryIndex;
        }
    }
}