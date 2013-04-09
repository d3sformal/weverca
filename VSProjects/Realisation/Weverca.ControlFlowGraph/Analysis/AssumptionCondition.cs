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

        public override int GetHashCode()
        {
            var sum = (int)Form;
            foreach (var part in Parts)
            {
                sum += part.GetHashCode();
            }
            return sum;
        }

        public override bool Equals(object obj)
        {
            var o = obj as AssumptionCondition;
            if (o == null)
                return false;

            var sameCount = Parts.Count() == o.Parts.Count();
            var sameEls = !Parts.Except(o.Parts).Any();
            var sameForms = Form == o.Form;

            return sameForms && sameCount && sameEls;
        }
    }
}
