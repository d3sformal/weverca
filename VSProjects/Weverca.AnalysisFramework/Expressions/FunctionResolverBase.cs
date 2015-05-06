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


﻿using System.IO;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework.Expressions
{
    /// <summary>
    /// Provides resolving of function names and function initialization
    /// </summary>
    public abstract class FunctionResolverBase
    {

        /// <summary>
        /// Gets current flow controller available for function resolver
        /// </summary>
        public FlowController Flow { get; private set; }

        /// <summary>
        /// Gets current output set of function resolver
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return Flow.OutSet; }
        }

        /// <summary>
        /// Gets current input set of function resolver
        /// </summary>
        public FlowInputSet InSet
        {
            get { return Flow.InSet; }
        }

        /// <summary>
        /// Gets InSet's snapshot
        /// </summary>
        public SnapshotBase InSnapshot
        {
            get { return InSet.Snapshot; }
        }

        /// <summary>
        /// Gets OutSet's snapshot
        /// </summary>
        public SnapshotBase OutSnapshot
        {
            get { return OutSet.Snapshot; }
        }

        /// <summary>
        /// Gets element which is currently evaluated
        /// </summary>
        public LangElement Element
        {
            get { return Flow.CurrentPartial; }
        }

        #region Template API methods for implementors

        /// <summary>
        /// Resolves return value of given program point graphs
        /// </summary>
        /// <param name="dispatchedExtensions">Program point graphs from call dispatch</param>
        /// <returns>Resolved return value</returns>
        public abstract MemoryEntry ResolveReturnValue(IEnumerable<ExtensionPoint> dispatchedExtensions);

        /// <summary>
        /// Initialization of call of function with given called object
        /// </summary>
        /// <param name="caller">Caller which caused initialization of call</param>
        /// <param name="extensionGraph">Graph representing initialized call</param>
        /// <param name="calledObject">Object that is available for call</param>
        /// <returns>Object on which call is processed. Null if there is no object</returns>
        public abstract MemoryEntry InitializeCalledObject(ProgramPointBase caller, ProgramPointGraph extensionGraph, MemoryEntry calledObject);

        /// <summary>
        /// Initialization of call of function with given declaration and arguments
        /// </summary>
        /// <param name="caller">Caller which caused initialization of call</param>
        /// <param name="extensionGraph">Graph representing initialized call</param>
        /// <param name="arguments">Call arguments</param>
        public abstract void InitializeCall(ProgramPointBase caller, ProgramPointGraph extensionGraph,
            MemoryEntry[] arguments);

        /// <summary>
        /// Is called when new object is created
        /// </summary>
        /// <param name="newObject">Entry of possible new objects created after construction</param>
        /// <param name="arguments">Arguments passed to constructor when creating a new object</param>
        /// <returns>Entry of objects, the same as <paramref name="newObject"/></returns>
        public abstract MemoryEntry InitializeObject(ReadSnapshotEntryBase newObject, MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for call of given name and arguments via flow controller
        /// </summary>
        /// <param name="name">Name of called function</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void Call(QualifiedName name, MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for indirect call of given name and arguments via flow controller
        /// </summary>
        /// <param name="name">Values indirectly representing name of called function</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void IndirectCall(MemoryEntry name, MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for method call of given name and arguments via flow controller
        /// </summary>
        /// <param name="calledObject">Object which method is called</param>
        /// <param name="name">Name of called method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void MethodCall(ReadSnapshotEntryBase calledObject, QualifiedName name,
            MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for indirect method call of given name and arguments
        /// via flow controller
        /// </summary>
        /// <param name="calledObject">Object which method is called</param>
        /// <param name="name">Values indirectly representing name of called method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void IndirectMethodCall(ReadSnapshotEntryBase calledObject, MemoryEntry name,
            MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for static method call of given name and arguments
        /// via flow controller
        /// </summary>
        /// <param name="typeName">Name of type where the static method is defined</param>
        /// <param name="name">Name of called static method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void StaticMethodCall(QualifiedName typeName, QualifiedName name,
            MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for static method call of given name and arguments
        /// via flow controller
        /// </summary>
        /// <param name="calledObject">Object which method is called</param>
        /// <param name="name">Name of called static method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void StaticMethodCall(ReadSnapshotEntryBase calledObject, QualifiedName name,
            MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for indirect static method call of given name and arguments
        /// via flow controller
        /// </summary>
        /// <param name="typeName">Name of type where the static method is defined</param>
        /// <param name="name">Values indirectly representing name of called static method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void IndirectStaticMethodCall(QualifiedName typeName, MemoryEntry name,
            MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for static method call of given name and arguments
        /// via flow controller
        /// </summary>
        /// <param name="calledObject">Object which method is called</param>
        /// <param name="name">Values indirectly representing name of called static method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void IndirectStaticMethodCall(ReadSnapshotEntryBase calledObject, MemoryEntry name,
           MemoryEntry[] arguments);



        #endregion

        #region Default implementations of simple routines

        /// <summary>
        /// Declare function into global context
        /// </summary>
        /// <param name="declaration">Declared function</param>
        public virtual void DeclareGlobal(FunctionDecl declaration)
        {
            OutSet.DeclareGlobal(declaration, Flow.CurrentScript);
        }

        /// <summary>
        /// Set return value to current call context via OutSet
        /// </summary>
        /// <param name="value">Value from return expression</param>
        /// <returns>Returned value</returns>
        public abstract MemoryEntry Return(MemoryEntry value);

        #endregion

        /// <summary>
        /// Set context for resolver
        /// </summary>
        /// <param name="flow">Flow controller for current context</param>
        internal void SetContext(FlowController flow)
        {
            Flow = flow;
        }

        #region Function based storages handling

        /// <summary>
        /// Sets return storage to given value.
        /// </summary>
        /// <param name="flowSet">the flow set in which the return storage to be set is located</param>
        /// <param name="returnValue">the value to be set to return storage</param>
        public static void SetReturn(FlowInputSet flowSet, MemoryEntry returnValue)
        {
            var outSnapshot = flowSet.Snapshot;
            var returnVar = outSnapshot.GetLocalControlVariable(SnapshotBase.ReturnValue);
            returnVar.WriteMemory(outSnapshot, returnValue);
        }

        /// <summary>
        /// Gets the value of return storage located in given output set.
        /// </summary>
        /// <param name="outSet">the output set in which the return storage is located</param>
        /// <returns>The memoryEntry of returned value</returns>
        protected static MemoryEntry GetReturn(FlowOutputSet outSet)
        {
            if (outSet == null)
                return null;

            var outSnapshot = outSet.Snapshot;
            var returnVar = outSnapshot.GetLocalControlVariable(SnapshotBase.ReturnValue);
            return returnVar.ReadMemory(outSnapshot);
        }

        #endregion

    }
}