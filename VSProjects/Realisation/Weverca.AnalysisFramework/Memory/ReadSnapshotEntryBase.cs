using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

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
        /// Determine that memory represented by current snapshot entry Is already defined.
        /// If not, reading memory returns UndefinedValue. But UndefinedValue can be returned
        /// even for defined memory entries - this can be used to distinct 
        /// between null/undefined semantic of PHP.
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        protected abstract bool isDefined(SnapshotBase context);

        /// <summary>
        /// Returns aliases that can be used for making alias join
        /// to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Aliases of current snapshot entry</returns>
        protected abstract IEnumerable<AliasEntry> aliases(SnapshotBase context);

        /// <summary>
        /// Read memory represented by current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Memory represented by current snapshot entry</returns>
        protected abstract MemoryEntry readMemory(SnapshotBase context);

        /// <summary>
        /// Read memory represented by given index identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="index">Identifier of an index</param>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Snapshot entry representing index resolving on current entry</returns>
        protected abstract ReadWriteSnapshotEntryBase readIndex(SnapshotBase context, MemberIdentifier index);

        /// <summary>
        /// Read memory represented by given field identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="field">Identifier of an field</param>
        /// <returns>Snapshot entry representing field resolving on current entry</returns>
        protected abstract ReadWriteSnapshotEntryBase readField(SnapshotBase context, VariableIdentifier field);

        /// <summary>
        /// Resolve method on current snapshot entry with given methodName
        /// </summary>
        /// <param name="context">Context where methods are resolved</param>
        /// <param name="methodName">Name of resolved method</param>
        /// <returns>Resolved methods</returns>
        protected abstract IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, QualifiedName methodName);

        /// <summary>
        /// Returns variables corresponding to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Variable identifier of current snapshot entry or null if entry doesn't belong to variable</returns>
        protected abstract VariableIdentifier getVariableIdentifier(SnapshotBase context);

        /// <summary>
        /// Iterate fields defined on object
        /// </summary>
        /// <param name="context">Context where fields are searched</param>
        /// <returns>Enumeration of available fields</returns>
        protected abstract IEnumerable<VariableIdentifier> iterateFields(SnapshotBase context);

        /// <summary>
        /// Iterate indexes defined on array
        /// </summary>
        /// <param name="context">Context where indexes are searched</param>
        /// <returns>Enumeration of available fields</returns>
        protected abstract IEnumerable<MemberIdentifier> iterateIndexes(SnapshotBase context);

        /// <summary>
        /// Determine that memory represented by current snapshot entry Is already defined.
        /// If not, reading memory returns UndefinedValue. But UndefinedValue can be returned
        /// even for defined memory entries - this can be used to distinct 
        /// between null/undefined semantic of PHP.
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        public bool IsDefined(SnapshotBase context)
        {
            return isDefined(context);
        }

        /// <summary>
        /// Returns aliases that can be used for making alias join
        /// to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Aliases of current snapshot entry</returns>
        public IEnumerable<AliasEntry> Aliases(SnapshotBase context)
        {
            return aliases(context);
        }

        /// <summary>
        /// Returns variables corresponding to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Variable identifier of current snapshot entry or null if entry doesn't belong to variable</returns>
        public VariableIdentifier GetVariableIdentifier(SnapshotBase context)
        {
            //TODO statistics reporting
            return getVariableIdentifier(context);
        }

        /// <summary>
        /// Read memory represented by current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>Memory represented by current snapshot entry</returns>
        public MemoryEntry ReadMemory(SnapshotBase context)
        {
            //TODO statistics reporting
            return readMemory(context);
        }

        /// <summary>
        /// Read memory represented by given index identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="index">Identifier of an index</param>
        /// <returns>Snapshot entries representing index resolving on current entry</returns>
        public ReadWriteSnapshotEntryBase ReadIndex(SnapshotBase context, MemberIdentifier index)
        {
            //TODO statistics reporting
            return readIndex(context, index);
        }

        /// <summary>
        /// Read memory represented by given field identifier resolved on current
        /// snapshot entry (resulting snapshot entry can encapsulate merging, alias resolving and
        /// other stuff based on nondeterminism of identifier and current snapshot entry)
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="field">Identifier of an field</param>
        /// <returns>Snapshot entries representing field resolving on current entry</returns>
        public ReadWriteSnapshotEntryBase ReadField(SnapshotBase context, VariableIdentifier field)
        {
            //TODO statistics reporting
            return readField(context, field);
        }

        /// <summary>
        /// Resolve method on current snapshot entry with given methodName
        /// </summary>
        /// <param name="context">Context where methods are resolved</param>
        /// <param name="methodName">Name of resolved method</param>
        /// <returns>Resolved methods</returns>
        public IEnumerable<FunctionValue> ResolveMethod(SnapshotBase context, QualifiedName methodName)
        {
            //TODO statistics reporting
            return resolveMethod(context, methodName);
        }

        /// <summary>
        /// Iterate fields defined on object
        /// </summary>
        /// <param name="context">Context where fields are searched</param>
        /// <returns>Enumeration of available fields</returns>
        public IEnumerable<VariableIdentifier> IterateFields(SnapshotBase context)
        {
            //TODO statistics reporting
            return iterateFields(context);
        }

        /// <summary>
        /// Iterate indexes defined on array
        /// </summary>
        /// <param name="context">Context where indexes are searched</param>
        /// <returns>Enumeration of available fields</returns>
        public IEnumerable<MemberIdentifier> IterateIndexes(SnapshotBase context)
        {
            //TODO statistics reporting
            return iterateIndexes(context);
        }
    }
}
