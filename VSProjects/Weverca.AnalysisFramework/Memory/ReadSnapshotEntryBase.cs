/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;

using Weverca.AnalysisFramework.Expressions;

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
        /// <returns><c>true</c> if memory is already defined.</returns>
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
        /// Resolve type of objects in snapshot entry
        /// </summary>
        /// <param name="context">Context where types are resolved</param>
        /// <returns>Resolved types</returns>
        protected abstract IEnumerable<TypeValue> resolveType(SnapshotBase context);

        /// <summary>
        /// Determine that memory represented by current snapshot entry Is already defined.
        /// If not, reading memory returns UndefinedValue. But UndefinedValue can be returned
        /// even for defined memory entries - this can be used to distinct
        /// between null/undefined semantic of PHP.
        /// </summary>
        /// <param name="context">Context snapshot where operation is proceeded</param>
        /// <returns><c>true</c> if memory is already defined.</returns>
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

            var allIndices = iterateIndexes(context);

            // filter out unknown field that has just undefined value
            var returnedIndices = new List<MemberIdentifier> (allIndices.Count());
            foreach (var index in allIndices) 
            {
                if (index.DirectName == null) 
                {
                    var tmp = readIndex (context, index).readMemory (context);
                    if (readIndex (context, index).readMemory (context).PossibleValues.Count() <= 1)
                        // unknown field that has just undefined value
                        continue;
                }

                returnedIndices.Add (index);
            }
            return returnedIndices;
        }

        /// <summary>
        /// Returns biggest integer index in arrays represented by this entry.
        /// </summary>
        /// <returns>The biggest integer index.</returns>
        /// <param name="context">Context where the index is searched.</param>
        /// <param name="evaluator">Expression evaluator.</param>
        public int BiggestIntegerIndex(SnapshotBase context, ExpressionEvaluatorBase evaluator)
        {
            int biggestIndex = -1;
            foreach (var index in iterateIndexes(context)) 
            {
                int currentIndex;
                if (evaluator.TryIdentifyInteger (index.DirectName, out currentIndex)) 
                {
                    if (currentIndex > biggestIndex)
                        biggestIndex = currentIndex;
                }

            }
            return biggestIndex;
        }

        /// <summary>
        /// Returns true if it is stored an associative array in this entry in given context.
        /// </summary>
        /// <returns><c>true</c>, if associative arrray is stored in this entry, <c>false</c> otherwise.</returns>
        /// <param name="context">Context where the array is searched.</param>
        public bool isAssociativeArrray(SnapshotBase context) 
        {
            foreach (var val in this.ReadMemory (context).PossibleValues)
            {
                if (val is AssociativeArray) 
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Resolve type of objects in snapshot entry
        /// </summary>
        /// <param name="context">Context where types are resolved</param>
        /// <returns>Resolved types</returns>
        public IEnumerable<TypeValue> ResolveType(SnapshotBase context)
        {
            //TODO statistics reporting
            return resolveType(context);
        }
    }
}