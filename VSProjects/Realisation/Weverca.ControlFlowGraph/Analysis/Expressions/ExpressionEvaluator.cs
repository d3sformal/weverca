using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis.Expressions
{
    public abstract class ExpressionEvaluator<FlowInfo>
    {

        public FlowInputSet<FlowInfo> InSet { get; private set; }

        public FlowOutputSet<FlowInfo> OutSet { get; private set; }

        public LangElement Element { get; internal set; }

        abstract public FlowInfo Copy(FlowInfo info);
        abstract public FlowInfo Assign(FlowInfo p1, FlowInfo p2);
        abstract public FlowInfo Declare(DirectVarUse x);
        abstract public FlowInfo BinaryEx(FlowInfo op1, Operations operation, FlowInfo op2);
        abstract public FlowInfo StringLiteral(StringLiteral x);

        public virtual FlowInfo ResolveVar(DirectVarUse x)
        {
            FlowInfo result;
            InSet.TryGetInfo(x.VarName, out result);
            return result;
        }



        internal void SetContext(FlowInputSet<FlowInfo> inSet, LangElement element, FlowOutputSet<FlowInfo> outSet)
        {
            InSet = inSet;
            OutSet = outSet;
            Element = element;
        }
    }
}
