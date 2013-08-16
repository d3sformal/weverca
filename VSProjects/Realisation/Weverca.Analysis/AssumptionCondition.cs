using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.Analysis
{
    /// <summary>
    /// Form of condition parts conjunction.
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

    /// <summary>
    /// Represents assumption condition in program flow.
    /// NOTE: Overrides GetHashCode and Equals methods so they can be used in hash containers.
    /// WARNING: All empty conditions with same form returns true for Equals, with same hashocode.
    /// </summary>
    public class AssumptionCondition
    {
        /// <summary>
        /// Holds initial parts for condition equality resolution
        /// </summary>
        private readonly Expression[] _initialParts;

        /// <summary>
        /// Form of condition parts conjunction.
        /// </summary>
        public readonly ConditionForm Form;
        /// <summary>
        /// Condition parts that are joined according to ConditionForm.
        /// </summary>
        public readonly IEnumerable<Expressions.Postfix> Parts;


        /// <summary>
        /// Creates assumption condition for given parts
        /// </summary>
        /// <param name="form">Form of condition parts conjunction</param>
        /// <param name="parts">Condition parts</param>
        internal AssumptionCondition(ConditionForm form, params Expression[] parts)
        {
            _initialParts = parts;
            Parts = from part in parts select Expressions.Converter.GetPostfix(part);
            Form = form;
        }

        public override int GetHashCode()
        {
            var sum = (int)Form;
            foreach (var part in _initialParts)
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


            var sameCount = _initialParts.Length == o._initialParts.Length;
            var sameEls = !_initialParts.Except(o._initialParts).Any();
            var sameForms = Form == o.Form;

            return sameForms && sameCount && sameEls;
        }
    }
}
