using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.AnalysisFramework.Memory
{
    /// <summary>
    /// Represents entry provided by snapshots. Provides accessing to memory based operations that CANNOT MODIFY 
    /// visible state of snapshot (read only operation abstraction)
    /// 
    /// <remarks>
    /// Even if this snapshot entry is read only, can be changed during time through 
    /// another write read snapshot entries
    /// </remarks>
    /// </summary>
    public abstract class ReadSnapshotEntryBase
    {
        /// <summary>
        /// Snapshot creating current entry
        /// Is used for storing statistics information
        /// </summary>
        protected readonly SnapshotBase OwningSnapshot;

        /// <summary>
        /// Read memory represented by current snapshot entry
        /// </summary>
        /// <returns>Memory represented by current snapshot entry</returns>
        protected abstract MemoryEntry readMemory();

        /// <summary>
        /// Determine that memory represented by current snapshot entry Is already defined.
        /// If not, reading memory returns UndefinedValue. But UndefinedValue can be returned
        /// even for defined memory entries - this can be used to distinct 
        /// between null/undefined semantic of PHP.
        /// </summary>
        protected abstract bool isDefined();

        /// <summary>
        /// Returns aliases that can be used for making alias join
        /// to current snapshot entry
        /// </summary>
        /// <returns>Aliases of current snapshot entry</returns>
        protected abstract IEnumerable<AliasEntry> aliases();

        /// <summary>
        /// Read memory represented by given index identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="index">Identifier of an index</param>
        /// <returns>Snapshot entries representing index resolving on current entry</returns>
        protected abstract IEnumerable<ReadSnapshotEntryBase> readIndex(MemberIdentifier index);

        /// <summary>
        /// Read memory represented by given field identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="field">Identifier of an field</param>
        /// <returns>Snapshot entries representing field resolving on current entry</returns>
        protected abstract IEnumerable<ReadSnapshotEntryBase> readField(MemberIdentifier field);

        /// <summary>
        /// Initialize read only snapshot entry
        /// </summary>
        /// 
        /// <param name="owningSnapshot">
        /// Snapshot creating current entry
        /// Is used for storing statistics information
        /// </param>
        protected ReadSnapshotEntryBase(SnapshotBase owningSnapshot)
        {
            OwningSnapshot = owningSnapshot;
        }

        /// <summary>
        /// Determine that memory represented by current snapshot entry Is already defined.
        /// If not, reading memory returns UndefinedValue. But UndefinedValue can be returned
        /// even for defined memory entries - this can be used to distinct 
        /// between null/undefined semantic of PHP.
        /// </summary>
        public bool IsDefined{get{return isDefined();}}

        /// <summary>
        /// Returns aliases that can be used for making alias join
        /// to current snapshot entry
        /// </summary>
        /// <returns>Aliases of current snapshot entry</returns>
        public IEnumerable<AliasEntry> Aliases { get { return aliases(); } }

        /// <summary>
        /// Read memory represented by current snapshot entry
        /// </summary>
        /// <returns>Memory represented by current snapshot entry</returns>
        public MemoryEntry ReadMemory()
        {            
            //TODO statistics reporting
            return readMemory();
        }

        /// <summary>
        /// Read memory represented by given index identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="index">Identifier of an index</param>
        /// <returns>Snapshot entries representing index resolving on current entry</returns>
        public ReadSnapshotEntryBase[] ReadIndex(MemberIdentifier index)
        {
            //TODO statistics reporting
            return readIndex(index).ToArray();
        }

        /// <summary>
        /// Read memory represented by given field identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="field">Identifier of an field</param>
        /// <returns>Snapshot entries representing field resolving on current entry</returns>
        public ReadSnapshotEntryBase[] ReadField(MemberIdentifier field)
        {
            //TODO statistics reporting
            return readField(field).ToArray();
        }
    }
}
