using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    /// <summary>
    /// Evaluates expressions during analysis
    /// </summary>
    public abstract class ExpressionEvaluator
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
        public LangElement Element { get; internal set; }

        #region Template API methods for implementors

        abstract public void AliasAssign(VariableName target, AliasValue alias);
        abstract public void Assign(VariableName target, MemoryEntry value);
        abstract public MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand);

        #endregion

        /// <summary>
        /// Set current evaluation context
        /// </summary>
        /// <param name="flow">Flow controller available for evaluation</param>
        /// <param name="element">Currently evaluated element</param>
        internal void SetContext(FlowController flow, LangElement element)
        {
            Flow = flow;
            Element = element;
        }
        
        #region Default implementation of simple routines

        virtual public MemoryEntry ResolveVariable(VariableName variable)
        {
            return InSet.ReadValue(variable);
        }
        
        virtual public AliasValue ResolveAlias(VariableName aliasedVariable)
        {
            return Flow.OutSet.CreateAlias(aliasedVariable);
        }

        virtual public MemoryEntry StringLiteral(StringLiteral x)
        {
            return new MemoryEntry(Flow.OutSet.CreateString(x.Value as String));
        }

        virtual public MemoryEntry IntLiteral(IntLiteral x)
        {
            return new MemoryEntry(Flow.OutSet.CreateInt((int)x.Value));
        }

        virtual public MemoryEntry IntLiteral(LongIntLiteral x)
        {
            return new MemoryEntry(Flow.OutSet.CreateLong((long)x.Value));
        }

        #endregion
    }
}
