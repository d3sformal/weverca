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


using PHP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Algorithm;
using Weverca.MemoryModels.ModularCopyMemoryModel.Interfaces.Structure;
using Weverca.MemoryModels.ModularCopyMemoryModel.Logging;
using Weverca.MemoryModels.ModularCopyMemoryModel.Memory;

namespace Weverca.MemoryModels.ModularCopyMemoryModel.SnapshotEntries
{
    /// <summary>
    /// Implements snapshot entry functionality in order to manipulate with variable acces paths in copy memory model.
    /// </summary>
    public class SnapshotEntry : ReadWriteSnapshotEntryBase, ICopyModelSnapshotEntry
    {
        /// <summary>
        /// Path for the current instance.
        /// </summary>
        readonly MemoryPath path;

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
            Snapshot.Logger.Log(snapshot, "write: " + this.ToString() + " value: " + value.ToString());

            IAssignAlgorithm algorithm = snapshot.Algorithms.AssignAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.WRITE);
            algorithm.Assign(snapshot, path, value, forceStrongWrite);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.WRITE);
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
            Snapshot.Logger.Log(snapshot, "write without copy:" + this.ToString());

            IAssignAlgorithm algorithm = snapshot.Algorithms.AssignAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.WRITE_WITHOUT_COPY);
            algorithm.WriteWithoutCopy(snapshot, path, value);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.WRITE_WITHOUT_COPY);
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
            Snapshot.Logger.Log(snapshot, "set alias: " + this.ToString() + " from: " + aliasedEntry.ToString());

            ICopyModelSnapshotEntry entry = ToEntry(aliasedEntry);

            IAssignAlgorithm algorithm = snapshot.Algorithms.AssignAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.SET_ALIAS);
            algorithm.AssignAlias(snapshot, path, entry.GetPath(snapshot));
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.SET_ALIAS);
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
            Snapshot.Logger.Log(snapshot, "is defined:" + this.ToString());

            IReadAlgorithm algorithm = snapshot.Algorithms.ReadAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.IS_DEFINED);
            var isDefined = algorithm.IsDefined(snapshot, path);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.IS_DEFINED);

            return isDefined;
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
            Snapshot.Logger.Log(snapshot, "read: " + this.ToString());

            IReadAlgorithm algorithm = snapshot.Algorithms.ReadAlgorithm;
            
            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.READ);
            MemoryEntry entry = algorithm.Read(snapshot, path);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.READ);

            Snapshot.Logger.LogToSameLine(" value: " + entry.ToString());
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
            Snapshot snapshot = ToSnapshot(context);
            Snapshot.Logger.Log(snapshot, "iterate fields: " + this.ToString());

            IReadAlgorithm algorithm = snapshot.Algorithms.ReadAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.ITERATE_FIELDS);
            MemoryEntry values = algorithm.Read(snapshot, path);
            var fields = algorithm.GetFields(snapshot, values);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.ITERATE_FIELDS);

            return fields;
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
            Snapshot snapshot = ToSnapshot(context);
            Snapshot.Logger.Log(snapshot, "iterate fields: " + this.ToString());

            IReadAlgorithm algorithm = snapshot.Algorithms.ReadAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.ITERATE_INDEXES);
            MemoryEntry values = algorithm.Read(snapshot, path);
            var indexes = algorithm.GetIndexes(snapshot, values);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.ITERATE_INDEXES);

            return indexes;
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
            Snapshot snapshot = ToSnapshot(context);
            Snapshot.Logger.Log(snapshot, "iterate fields: " + this.ToString());

            IReadAlgorithm algorithm = snapshot.Algorithms.ReadAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.RESOLVE_TYPE);
            MemoryEntry values = algorithm.Read(snapshot, path);
            var types = algorithm.GetObjectType(snapshot, values);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.RESOLVE_TYPE);

            return types;
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
            Snapshot.Logger.Log(snapshot, "iterate filds: " + this.ToString());

            IReadAlgorithm algorithm = snapshot.Algorithms.ReadAlgorithm;

            Snapshot.Benchmark.StartAlgorithm(snapshot, algorithm, AlgorithmType.RESOLVE_METHOD);
            MemoryEntry values = algorithm.Read(snapshot, path);
            algorithm.Read(snapshot, path);
            var methods = algorithm.GetMethod(snapshot, values, methodName);
            Snapshot.Benchmark.FinishAlgorithm(snapshot, algorithm, AlgorithmType.RESOLVE_METHOD);

            return methods;
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

        /// <summary>
        /// Gets the path of this snapshot entry.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>
        /// The path of this snapshot entry.
        /// </returns>
        public MemoryPath GetPath(Snapshot snapshot)
        {
            return path;
        }
    }
}