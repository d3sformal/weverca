using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.Analysis.Expressions
{
    public abstract class ExpressionEvaluator
    {
        public FlowControler Flow{ get; private set; }
        public LangElement Element { get; internal set; }

        abstract public void AliasAssign(VariableName target, AliasValue alias);
        abstract public void Assign(VariableName target, MemoryEntry value);
        
        abstract public MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand);

        abstract public MemoryEntry ResolveVariable(VariableName variable);

        internal void SetContext(FlowControler flow, LangElement element)
        {
            Flow = flow;
            Element = element;
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
    }
}
