using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;

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
        ISnapshotDataProxy CreateEmptyInstance(ModularMemoryModelFactories factories);

        /// <summary>
        /// Creates new snapshot data container as copy of the given one.
        /// Copied object mustn't interfere with source.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        /// <returns>New object with inner scructure as copy of given object.</returns>
        ISnapshotDataProxy CopyInstance(ISnapshotDataProxy oldData);

        /// <summary>
        /// Creates new snapshot structure container as copy containing data from given inner container.
        /// Copied object mustn't interfere with source.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="oldData">The old data.</param>
        /// <returns>New snapshot structure container as copy containing data from given inner container.</returns>
        ISnapshotDataProxy CreateNewInstanceWithData(IReadOnlySnapshotData oldData);
    }
}
