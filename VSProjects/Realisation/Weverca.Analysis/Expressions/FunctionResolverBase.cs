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
        public abstract MemoryEntry ResolveReturnValue(ProgramPointGraph[] programPointGraphs);

        public virtual void DeclareGlobal(TypeDecl declaration)
        {
            OutSet.DeclareGlobal(declaration);
        }

        public virtual void DeclareGlobal(FunctionDecl declaration)
        {
            OutSet.DeclareGlobal(declaration);
        }

        public virtual MemoryEntry Return(FlowOutputSet outSet, MemoryEntry value)
        {
            OutSet.Assign(outSet.ReturnValue, value);
            return value;
        }

        public abstract void MethodCall(MemoryEntry calledObject, QualifiedName name, MemoryEntry[] arguments);

        public abstract void Call(QualifiedName name, MemoryEntry[] arguments);

        public abstract void IndirectMethodCall(MemoryEntry calledObject, MemoryEntry name, MemoryEntry[] arguments);

        public abstract void IndirectCall(MemoryEntry name, MemoryEntry[] arguments);


        public abstract void InitializeCall(FlowOutputSet callInput, LangElement declaration, MemoryEntry[] arguments);

        internal void SetContext(FlowController flow)
        {
            Flow = flow;
        }
    }
}
