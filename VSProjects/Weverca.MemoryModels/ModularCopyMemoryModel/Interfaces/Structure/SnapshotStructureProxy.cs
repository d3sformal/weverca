using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure
{
    /// <summary>
    /// Proxy object for snapshot structure container. This object is used to distinguish readonly or
    /// writeable access to structure container.
    /// </summary>
    public interface ISnapshotStructureProxy
    {
        /// <summary>
        /// Gets or sets a value indicating whether structural changes are allowed or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if structural changes are forbiden; otherwise, <c>false</c>.
        /// </value>
        bool Locked { get; set; }

        /// <summary>
        /// Gets the snasphot structure container for read only access.
        /// </summary>
        /// <value>
        /// The read only snapshot structure.
        /// </value>
        IReadOnlySnapshotStructure Readonly { get; }

        /// <summary>
        /// Gets the snapshot structure container for access which allows modifications.
        /// </summary>
        /// <value>
        /// The writeable snapshot structure.
        /// </value>
        IWriteableSnapshotStructure Writeable { get; }

        /// <summary>
        /// Gets a value indicating whether this instance was used only in readonly mode or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance was used only in readonly mode; otherwise, <c>false</c>.
        /// </value>
        bool IsReadonly { get; }
    }
}
