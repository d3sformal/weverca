using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.ControlFlowGraph;
using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    public abstract class FunctionResolverBase
    {
        #region Template API methods for implementors

        /// <summary>
        /// Current flow controller available for expression evaluation
        /// </summary>
        public FlowController Flow { get; private set; }

        /// <summary>
        /// Current output set of expression evaluation
        /// </summary>
        public FlowOutputSet OutSet { get { return Flow.OutSet; } }

        /// <summary>
        /// Current input set of expression evaluation
        /// </summary>
        public FlowInputSet InSet { get { return Flow.InSet; } }

        /// <summary>
        /// Element which is currently evaluated
        /// </summary>
        public LangElement Element { get { return Flow.CurrentPartial; } }

        /// <summary>
        /// Resolves return value of given program point graphs
        /// </summary>
        /// <param name="programPointGraphs">Program point graphs from call dispatch</param>
        /// <returns>Resolved return value</returns>
        public abstract MemoryEntry ResolveReturnValue(IEnumerable<ProgramPointGraph> programPointGraphs);


        #region Default implementations of simple routines

        /// <summary>
        /// Declare type into global context
        /// </summary>
        /// <param name="declaration">Declared type</param>
        public virtual void DeclareGlobal(TypeDecl declaration)
        {
            var type = OutSet.CreateType(declaration);
            OutSet.DeclareGlobal(type);
        }

        /// <summary>
        /// Declare function into global context
        /// </summary>
        /// <param name="declaration">Declared function</param>
        public virtual void DeclareGlobal(FunctionDecl declaration)
        {
            OutSet.DeclareGlobal(declaration);
        }

        /// <summary>
        /// Set return value to current call context via OutSet
        /// </summary>        
        /// <param name="value">Value from return expression</param>
        /// <returns>Returned value</returns>
        public virtual MemoryEntry Return(MemoryEntry value)
        {
            OutSet.Assign(OutSet.ReturnValue, value);
            return value;
        }

        /// <summary>
        /// Initialization of call of function with given declaration and arguments
        /// </summary>
        /// <param name="callInput">Input of initialized call</param>
        /// <param name="extensionGraph">Graph representing initialized call</param>
        /// <param name="arguments">Call arguments</param>
        public abstract void InitializeCall(FlowOutputSet callInput, ProgramPointGraph extensionGraph, MemoryEntry[] arguments);

        /// <summary>
        /// Is called when new object is created
        /// </summary>
        /// <param name="newObject"></param>
        /// <param name="arguments"></param>
        public abstract MemoryEntry InitializeObject(MemoryEntry newObject, MemoryEntry[] arguments);
        
        /// <summary>
        /// Builds program point extension for call of given name and arguments via flow controller
        /// </summary>
        /// <param name="name">Name of called function</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void Call(QualifiedName name, MemoryEntry[] arguments);
        
        /// <summary>
        /// Builds program point extension for indirect call of given name and arguments via flow controller
        /// </summary>
        /// <param name="name">Name of called function</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void IndirectCall(MemoryEntry name, MemoryEntry[] arguments);

        /// <summary>
        /// Builds program point extension for method call of given name and arguments via flow controller
        /// </summary>
        /// <param name="calledObject">Object which method is called</param>
        /// <param name="name">Name of called method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void MethodCall(MemoryEntry calledObject, QualifiedName name, MemoryEntry[] arguments);
        
        /// <summary>
        /// Builds program point extension for indirect method call of given name and arguments via flow conroller
        /// </summary>
        /// <param name="calledObject">Object which method is called</param>
        /// <param name="name">Name of called method</param>
        /// <param name="arguments">Arguments of call</param>
        public abstract void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name, MemoryEntry[] arguments);
        
        #endregion

        #endregion

        /// <summary>
        /// Set context for resolver
        /// </summary>
        /// <param name="flow">Flow controller for current context</param>
        internal void SetContext(FlowController flow)
        {
            Flow = flow;
        }

    }
}
