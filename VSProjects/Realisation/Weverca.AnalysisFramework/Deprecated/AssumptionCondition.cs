using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis
{

    /// <summary>
    /// Represents assumption condition in program flow.
    /// NOTE: Overrides GetHashCode and Equals methods so they can be used in hash containers.
    /// WARNING: All empty conditions with same form returns true for Equals, with same hashocode.
    /// </summary>
    public class AssumptionCondition_deprecated
    {
        /// <summary>
        /// Form of condition parts joining.
        /// </summary>
        public readonly ConditionForm Form;
        /// <summary>
        /// Condition parts that are joined according to Form.
        /// </summary>
        public readonly IEnumerable<Expression> Parts;

        public AssumptionCondition_deprecated(ConditionForm form, params Expression[] parts)
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
            var o = obj as AssumptionCondition_deprecated;
            if (o == null)
                return false;

            var sameCount = Parts.Count() == o.Parts.Count();
            var sameEls = !Parts.Except(o.Parts).Any();
            var sameForms = Form == o.Form;

            return sameForms && sameCount && sameEls;
        }
    }
}
