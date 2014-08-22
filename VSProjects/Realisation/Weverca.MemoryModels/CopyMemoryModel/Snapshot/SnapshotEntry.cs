using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHP.Core;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.MemoryModels.CopyMemoryModel
{
    /// <summary>
    /// Implements snapshot entry functionality in order to manipulate with variable acces paths in copy memory model.
    /// </summary>
    public class SnapshotEntry : ReadWriteSnapshotEntryBase, ICopyModelSnapshotEntry
    {
        /// <summary>
        /// Path for the current instance.
        /// </summary>
        MemoryPath path;

        /// <summary>
        /// Identification of parent variable.
        /// </summary>
        private AnalysisFramework.VariableIdentifier variableId;

        #region Creators of snapshot entry

        /// <summary>
        /// Creates the snapshot entry for the given variable name.
        /// </summary>
        /// <param name="variable">The variable name.</param>
        /// <param name="global">Determines whether variable is global or local.</param>
        internal static ReadWriteSnapshotEntryBase CreateVariableEntry(AnalysisFramework.VariableIdentifier variable, GlobalContext global)
        {
            return CreateVariableEntry(variable, global, Snapshot.GLOBAL_CALL_LEVEL);
        }

        /// <summary>
        /// Creates the snapshot entry for the given variable name.
        /// </summary>
        /// <param name="variable">The variable name.</param>
        /// <param name="global">Determines whether variable is global or local.</param>
        /// <param name="callLevel">The call level.</param>
        internal static ReadWriteSnapshotEntryBase CreateVariableEntry(AnalysisFramework.VariableIdentifier variable, GlobalContext global, int callLevel)
        {
            MemoryPath path;
            if (variable.IsUnknown)
            {
                path = MemoryPath.MakePathAnyVariable(global, callLevel);
            }
            else
            {
                var names = from name in variable.PossibleNames select name.Value;
                path = MemoryPath.MakePathVariable(names, global, callLevel);
            }

            return new SnapshotEntry(path, variable);
        }

        /// <summary>
        /// Creates the snapshot entry for the given variable name.
        /// </summary>
        /// <param name="name">The name of control variable.</param>
        /// <param name="global">Determines whether variable is global or local.</param>
        internal static ReadWriteSnapshotEntryBase CreateControlEntry(VariableName name, GlobalContext global)
        {
            return CreateControlEntry(name, global, Snapshot.GLOBAL_CALL_LEVEL);
        }

        /// <summary>
        /// Creates the snapshot entry for the given variable name.
        /// </summary>
        /// <param name="name">The name of control variable.</param>
        /// <param name="global">Determines whether variable is global or local.</param>
        /// <param name="callLevel">The call level.</param>
        /// <returns>New snapshot entry for the given variable name.</returns>
        internal static ReadWriteSnapshotEntryBase CreateControlEntry(VariableName name, GlobalContext global, int callLevel)
        {
            MemoryPath path = MemoryPath.MakePathControl(new string[] { name.ToString() }, global, callLevel);
            return new SnapshotEntry(path);
        }

        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotEntry"/> class.
        /// </summary>
        /// <param name="path">The variable acces path.</param>
        internal SnapshotEntry(MemoryPath path)
        {
            this.path = path;

            this.variableId = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotEntry"/> class.
        /// </summary>
        /// <param name="path">The variable acces path.</param>
        /// <param name="variableId">The variable unique identifier.</param>
        private SnapshotEntry(MemoryPath path, AnalysisFramework.VariableIdentifier variableId)
        {
            this.path = path;
            this.variableId = variableId;
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
            MemoryPath newPath;
            if (index.IsAny) {
                newPath = MemoryPath.MakePathAnyIndex (path);
            } else if (index.IsUnknown) 
            {
                newPath = MemoryPath.MakePathUnknownIndex (path);
            } else
            {
                newPath = MemoryPath.MakePathIndex(path, index.PossibleNames);
            }

            return new SnapshotEntry(newPath, variableId);
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
            MemoryPath newPath;
            if (field.IsUnknown)
            {
                newPath = MemoryPath.MakePathAnyField(path);
            }
            else
            {
                var names = from name in field.PossibleNames select name.Value;
                newPath = MemoryPath.MakePathField(path, names);
            }

            return new SnapshotEntry(newPath, variableId);
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
            Snapshot snapshot = ToSnapshot(context);
            SnapshotLogger.append(context, "write: " + this.ToString() + " value: " + value.ToString());

            switch (snapshot.CurrentMode)
            {
                case SnapshotMode.MemoryLevel:
                    writeMemoryNormal(snapshot, value, forceStrongWrite);
                    break;

                case SnapshotMode.InfoLevel:
                    writeMemoryInfo(snapshot, value, forceStrongWrite);
                    break;

                default:
                    throw new NotSupportedException("Current mode: " + snapshot.CurrentMode);
            }
        }

        /// <summary>
        /// Writes the memory information.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="value">The value.</param>
        /// <param name="forceStrongWrite">if set to <c>true</c> [force strong write].</param>
        private void writeMemoryInfo(Snapshot snapshot, MemoryEntry value, bool forceStrongWrite)
        {
            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            if (forceStrongWrite)
            {
                collector.SetAllToMust();
            }

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(snapshot);
            worker.Assign(collector, value);
        }

        /// <summary>
        /// Writes the memory normal.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="value">The value.</param>
        /// <param name="forceStrongWrite">if set to <c>true</c> [force strong write].</param>
        private void writeMemoryNormal(Snapshot snapshot, MemoryEntry value, bool forceStrongWrite)
        {
            TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, value);

            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            if (forceStrongWrite)
            {
                collector.SetAllToMust();
            }

            AssignWorker worker = new AssignWorker(snapshot);
            worker.Assign(collector, temporaryIndex);

            snapshot.ReleaseTemporary(temporaryIndex);
        }

        /// <summary>
        /// Write given value at memory represented by snapshot entry and doesn't process any
        /// array copy. Is needed for correct increment/decrement semantic.
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="value">Written value</param>
        protected override void writeMemoryWithoutCopy(SnapshotBase context, MemoryEntry value)
        {
            Snapshot snapshot = ToSnapshot(context);
            SnapshotLogger.append(context, "write without copy:" + this.ToString());

            AssignCollector collector = new AssignCollector(snapshot);
            collector.ProcessPath(path);

            AssignWithoutCopyWorker worker = new AssignWithoutCopyWorker(snapshot);
            worker.Assign(collector, value);
        }

        /// <summary>
        /// Set aliases to current snapshot entry. Aliases can be set even to those entries
        /// that doesn't belongs to any variable, field,..
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <param name="aliasedEntry">Snapshot entry which will be aliased from current entry</param>
        protected override void setAliases(SnapshotBase context, ReadSnapshotEntryBase aliasedEntry)
        {
            Snapshot snapshot = ToSnapshot(context);
            SnapshotLogger.append(context, "set alias: " + this.ToString() + " from: " + aliasedEntry.ToString());

            if (snapshot.CurrentMode == SnapshotMode.InfoLevel)
            {
                return;
            }

            ICopyModelSnapshotEntry entry = ToEntry(aliasedEntry);
            AliasData data = entry.CreateAliasToEntry(snapshot);

            AssignCollector collector = new AssignCollector(snapshot);
            collector.AliasesProcessing = AliasesProcessing.BeforeCollecting;
            collector.ProcessPath(path);

            AssignAliasWorker worker = new AssignAliasWorker(snapshot);
            worker.AssignAlias(collector, data);

            data.Release(snapshot);
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
        /// <returns>True whether memory represented by current snapshot entry Is already defined.</returns>
        protected override bool isDefined(SnapshotBase context)
        {
            Snapshot snapshot = ToSnapshot(context);
            SnapshotLogger.append(context, "is defined:" + this.ToString());

            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            return collector.IsDefined;
        }

        /// <summary>
        /// Returns aliases that can be used for making alias join
        /// to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>
        /// Aliases of current snapshot entry
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override IEnumerable<AliasEntry> aliases(SnapshotBase context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read memory represented by current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>
        /// Memory represented by current snapshot entry
        /// </returns>
        protected override MemoryEntry readMemory(SnapshotBase context)
        {
            Snapshot snapshot = ToSnapshot(context);
            SnapshotLogger.append(context, "read: " + this.ToString());

            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            ReadWorker worker = new ReadWorker(snapshot);
            MemoryEntry entry = worker.ReadValue(collector);
            SnapshotLogger.appendToSameLine(" value: " + entry.ToString());

            return entry;
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
        /// Resolve method on current snapshot entry with given methodName
        /// </summary>
        /// <param name="context">Context where methods are resolved</param>
        /// <param name="methodName">Name of resolved method</param>
        /// <returns>
        /// Resolved methods
        /// </returns>
        protected override IEnumerable<FunctionValue> resolveMethod(SnapshotBase context, QualifiedName methodName)
        {
            Snapshot snapshot = ToSnapshot(context);
            SnapshotLogger.append(context, "resolve method - path: " + this.ToString() + " method: " + methodName); 
            
            ReadCollector collector = new ReadCollector(snapshot);
            collector.ProcessPath(path);

            ReadWorker worker = new ReadWorker(snapshot);
            MemoryEntry memory = worker.ReadValue(collector);

            return snapshot.resolveMethod(memory, methodName);
        }

        /// <summary>
        /// Returns variables corresponding to current snapshot entry
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns>
        /// Variable identifier of current snapshot entry or null if entry doesn't belong to variable
        /// </returns>
        /// <exception cref="System.Exception">No variable identifier set for this object.</exception>
        protected override AnalysisFramework.VariableIdentifier getVariableIdentifier(SnapshotBase context)
        {
            if (variableId != null)
            {
                return variableId;
            }
            else
            {
                throw new Exception("No variable identifier set for this object.");
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Cnverts context base variable into the copy memory model snapshot.
        /// </summary>
        /// <param name="context">The context variable.</param>
        /// <returns>Given context parameter converted into copy memory model snapshot.</returns>
        /// <exception cref="System.ArgumentException">Context parametter is not of type Weverca.MemoryModels.CopyMemoryModel.Snapshot</exception>
        public static Snapshot ToSnapshot(ISnapshotReadonly context)
        {
            Snapshot snapshot = context as Snapshot;

            if (snapshot != null)
            {
                return snapshot;
            }
            else
            {
                throw new ArgumentException("Context parametter is not of type Weverca.MemoryModels.CopyMemoryModel.Snapshot");
            }
        }

        /// <summary>
        /// Converts base snapshot entry variable into copy memory model snapshot entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Given entry parameter converted into copy memory model entry.</returns>
        /// <exception cref="System.ArgumentException">Entry parametter is not of type Weverca.MemoryModels.CopyMemoryModel.ICopyModelSnapshotEntry</exception>
        internal static ICopyModelSnapshotEntry ToEntry(ReadSnapshotEntryBase entry)
        {
            ICopyModelSnapshotEntry copyEntry = entry as ICopyModelSnapshotEntry;

            if (copyEntry != null)
            {
                return copyEntry;
            }
            else
            {
                throw new ArgumentException("Entry parametter is not of type Weverca.MemoryModels.CopyMemoryModel.ICopyModelSnapshotEntry");
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
            return path.ToString();
        }

        /// <summary>
        /// Creates the alias to this entry and returnes data which can be used to aliasing the target.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>
        /// Alias data fro the newly created aliases.
        /// </returns>
        public AliasData CreateAliasToEntry(Snapshot snapshot)
        {
            //Collect alias indexes
            AssignCollector indexesCollector = new AssignCollector(snapshot);
            indexesCollector.ProcessPath(path);

            //Memory locations where to get data from
            ReadCollector valueCollector = new ReadCollector(snapshot);
            valueCollector.ProcessPath(path);

            //Get data from locations
            ReadWorker worker = new ReadWorker(snapshot);
            MemoryEntry value = worker.ReadValue(valueCollector);

            //Makes deep copy of data to prevent changes after assign alias
            TemporaryIndex temporaryIndex = snapshot.CreateTemporary();
            MergeWithinSnapshotWorker mergeWorker = new MergeWithinSnapshotWorker(snapshot);
            mergeWorker.MergeMemoryEntry(temporaryIndex, value);

            AliasData data = new AliasData(indexesCollector.MustIndexes, indexesCollector.MayIndexes, temporaryIndex);
            data.TemporaryIndexToRealease(temporaryIndex);

            return data;
        }

        /// <summary>
        /// Reads the memory.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>
        /// Memory represented by current snapshot entry.
        /// </returns>
        public MemoryEntry ReadMemory(Snapshot snapshot)
        {
            return this.readMemory(snapshot);
        }
    }
}
