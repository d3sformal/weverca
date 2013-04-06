using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{

    public enum ConditionForm { Some, All, None }
    public class AssumptionCondition
    {
        public readonly ConditionForm Form;
        public readonly IEnumerable<Expression> Parts;

        public AssumptionCondition(ConditionForm form, params Expression[] parts)
        {
            Parts = parts;
            Form = form;
        }
    }
}
