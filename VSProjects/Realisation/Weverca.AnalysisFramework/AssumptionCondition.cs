using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

namespace Weverca.AnalysisFramework
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
        SomeNot,

        ExactlyOne,

        NotExactlyOne
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
    }
}
