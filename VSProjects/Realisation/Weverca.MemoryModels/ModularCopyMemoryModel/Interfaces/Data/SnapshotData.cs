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
using Weverca.MemoryModels.ModularCopyMemoryModel.Implementation.Common;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data
{
    /// <summary>
    /// Factory object of snapshot data container. This is the only way memory model will
    /// create instances of snapshot data container.
    /// 
    /// Supports creating new empty instance or copiing existing one.
    /// </summary>
    public interface ISnapshotDataFactory
    {
        /// <summary>
        /// Creates the empty instance of snashot objects.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New object with empty inner structure.</returns>
        ISnapshotDataProxy CreateEmptyInstance(Snapshot snapshot);

        /// <summary>
        /// Creates new snapshot data container as copy of the given one.
        /// Copied object mustn't interfere with source.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        /// <returns>New object with inner scructure as copy of given object.</returns>
        ISnapshotDataProxy CopyInstance(Snapshot snapshot, ISnapshotDataProxy oldData);
    }

    /// <summary>
    /// Proxy object for snapshot data container. This object is used to distinguish readonly or
    /// writeable access to data container.
    /// </summary>
    public interface ISnapshotDataProxy
    {
        /// <summary>
        /// Gets the snasphot data container for read only access.
        /// </summary>
        /// <value>
        /// The read only snapshot data.
        /// </value>
        IReadOnlySnapshotData Readonly { get; }

        /// <summary>
        /// Gets the snapshot data container for access which allows modifications.
        /// </summary>
        /// <value>
        /// The writeable snapshot data.
        /// </value>
        IWriteableSnapshotData Writeable { get; }        
        
        /// <summary>
        /// Gets a value indicating whether this instance was used only in readonly mode or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance was used only in readonly mode; otherwise, <c>false</c>.
        /// </value>
        bool IsReadonly { get; }
    }

    /// <summary>
    /// Definition of basic read only operation for snapshot data container.  
    /// </summary>
    public interface IReadOnlySnapshotData
    {
        /// <summary>
        /// Gets the identifier of snapshot data object.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int DataId { get; }

        /// <summary>
        /// Gets a value indicating whether data were changed on commit.
        /// </summary>
        /// <value>
        ///   <c>true</c> if data were changed on commit; otherwise, <c>false</c>.
        /// </value>
        bool DiffersOnCommit { get; }

        /// <summary>
        /// Gets the index change tracker.
        /// </summary>
        /// <value>
        /// The index change tracker.
        /// </value>
        IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotData> IndexChangeTracker { get; }

        /// <summary>
        /// Gets the list of indexes for which are defined data in this container.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        IEnumerable<MemoryIndex> Indexes { get; }

        /// <summary>
        /// Gets the set of stored values for indexes.
        /// </summary>
        /// <value>
        /// The set of data.
        /// </value>
        IEnumerable<KeyValuePair<MemoryIndex, MemoryEntry>> Data { get; }

        /// <summary>
        /// Gets the memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>Memory entry for the given index.</returns>
        MemoryEntry GetMemoryEntry(MemoryIndex index);

        /// <summary>
        /// Tries to get memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="entry">The entry.</param>
        /// <returns>True whether collection contains memory entry for the given memory index.</returns>
        bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry);
    }

    /// <summary>
    /// Definition of operations for snapshot data object which modifies inner structure.
    /// </summary>
    public interface IWriteableSnapshotData : IReadOnlySnapshotData
    {        
        /// <summary>
        /// Sets the value indicating whether data were changed on commit.
        /// </summary>
        /// <param name="isDifferent">if set to <c>true</c> this data were changed on commit.</param>
        void SetDiffersOnCommit(bool isDifferent);

        /// <summary>
        /// Sets the memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="memoryEntry">The memory entry.</param>
        void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry);

        /// <summary>
        /// Removes the memory entry.
        /// </summary>
        /// <param name="index">The index.</param>
        void RemoveMemoryEntry(MemoryIndex index);

        /// <summary>

        /// Reinitializes the index change tracker and sets the parent tracker to be given structure tracker.
        /// </summary>
        /// <param name="parentSnapshotData">The parent snapshot data.</param>
        void ReinitializeIndexTracker(IReadOnlySnapshotData parentSnapshotData);
    }

    /// <summary>
    /// Basic abstract implementation of snapshot data container.
    /// 
    /// Implements unique identifiers and snapshot storing.
    /// </summary>
    public abstract class AbstractSnapshotData : IReadOnlySnapshotData, IWriteableSnapshotData
    {
        /// <summary>
        /// Incremental counter for data unique identifier.
        /// </summary>
        private static int DATA_ID = 0;

        /// <summary>
        /// Gets the snapshot.
        /// </summary>
        /// <value>
        /// The snapshot.
        /// </value>
        protected Snapshot Snapshot { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractSnapshotData" /> class.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        public AbstractSnapshotData(Snapshot snapshot)
        {
            DataId = DATA_ID++;
            Snapshot = snapshot;
        }

        /// <inheritdoc />
        public int DataId
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public bool DiffersOnCommit
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public void SetDiffersOnCommit(bool isDifferent)
        {
            DiffersOnCommit = isDifferent;
        }

        /// <inheritdoc />
        public abstract IEnumerable<MemoryIndex> Indexes
        {
            get;
        }

        /// <inheritdoc />
        public abstract IEnumerable<KeyValuePair<MemoryIndex, MemoryEntry>> Data
        {
            get;
        }

        /// <inheritdoc />
        public abstract MemoryEntry GetMemoryEntry(MemoryIndex index);

        /// <inheritdoc />
        public abstract bool TryGetMemoryEntry(MemoryIndex index, out MemoryEntry entry);

        /// <inheritdoc />
        public abstract void SetMemoryEntry(MemoryIndex index, MemoryEntry memoryEntry);

        /// <inheritdoc />
        public abstract void RemoveMemoryEntry(MemoryIndex index);

        /// <inheritdoc />
        public virtual IReadonlyChangeTracker<MemoryIndex, IReadOnlySnapshotData> IndexChangeTracker
        {
            get { return null; }
        }

        /// <inheritdoc />
        public virtual void ReinitializeIndexTracker(IReadOnlySnapshotData parentSnapshotData)
        {

        }
    }
}