using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Data
{
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
}
