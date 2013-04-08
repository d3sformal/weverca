using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Form of conjunction condition parts.
    /// </summary>
    public enum ConditionForm
    {
        /// <summary>
        /// Any true part is enough.
        /// </summary>
        Some,
        /// <summary>
        /// All parts has to be true.
        /// </summary>
        All,
        /// <summary>
        /// None part can be true.
        /// </summary>
        None,
        /// <summary>
        /// Some part has to be false.
        /// </summary>
        SomeNot
    }


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
