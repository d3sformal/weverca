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

        abstract public void Assign(VariableName target, MemoryEntry value);
        abstract public void Declare(DirectVarUse x);
        abstract public MemoryEntry BinaryEx(MemoryEntry leftOperand, Operations operation, MemoryEntry rightOperand);
        abstract public MemoryEntry StringLiteral(StringLiteral x);
        abstract public MemoryEntry ResolveVariable(VariableName variable);

        internal void SetContext(FlowControler flow, LangElement element)
        {
            Flow = flow;
            Element = element;
        }
    }
}
