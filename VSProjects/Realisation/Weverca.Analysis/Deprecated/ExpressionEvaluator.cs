using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis.Expressions
{
    public abstract class ExpressionEvaluator<FlowInfo>
    {

        public FlowControler<FlowInfo> Flow{ get; private set; }

        public LangElement Element { get; internal set; }

        abstract public FlowInfo Copy(FlowInfo info);
        abstract public FlowInfo Assign(FlowInfo p1, FlowInfo p2);
        abstract public FlowInfo Declare(DirectVarUse x);
        abstract public FlowInfo BinaryEx(FlowInfo op1, Operations operation, FlowInfo op2);
        abstract public FlowInfo StringLiteral(StringLiteral x);

        public virtual FlowInfo ResolveVar(DirectVarUse x)
        {
            FlowInfo result;
            Flow.InSet.TryGetInfo(x.VarName, out result);
            return result;
        }



        internal void SetContext(FlowControler<FlowInfo> flow, LangElement element)
        {
            Flow = flow;
            Element = element;
        }
    }
}
