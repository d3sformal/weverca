﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Factory object of snapshot structure container. This is the only way memory model will
    /// create instances of snapshot structure container.
    /// 
    /// Supports creating new empty instance or copiing existing one.
    /// </summary>
    public interface ISnapshotStructureFactory
    {
        /// <summary>
        /// Creates the empty instance of snapshot structure object.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New object with empty inner structure.</returns>
        ISnapshotStructureProxy CreateEmptyInstance(Snapshot snapshot);

        /// <summary>
        /// Creates new snapshot structure container as copy of the given one.
        /// Copied object mustn't interfere with source.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        /// <returns>New object with inner scructure as copy of given object.</returns>
        ISnapshotStructureProxy CopyInstance(Snapshot snapshot, ISnapshotStructureProxy oldData);

        /// <summary>
        /// Creates new snapshot structure container with empty global context.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>New object with empty global context.</returns>
        ISnapshotStructureProxy CreateGlobalContextInstance(Snapshot snapshot);

        /// <summary>
        /// Creates new snapshot structure container as copy containing data from given inner container.
        /// Copied object mustn't interfere with source.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        /// <returns>New object with inner scructure as copy of given object.</returns>
        ISnapshotStructureProxy CreateNewInstanceWithData(Snapshot snapshot, IReadOnlySnapshotStructure oldData);
    }
}
