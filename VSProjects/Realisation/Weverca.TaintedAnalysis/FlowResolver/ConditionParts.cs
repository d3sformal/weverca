using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.FlowResolver
{
    /// <summary>
    /// This class holds context of full condition and realise the assumptions.
    /// </summary>
    class ConditionParts
    {
        #region Members

        List<ConditionPart> trueParts = new List<ConditionPart>();
        List<ConditionPart> falseParts = new List<ConditionPart>();
        List<ConditionPart> unknownParts = new List<ConditionPart>();
        //TODO: something for holding values of variables. It might be useful to extend memory model.

        FlowOutputSet flowOutputSet;
        ConditionForm conditionForm;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the count of the parts which are evaluated as <c>true</c>.
        /// </summary>
        public int TruePartsCount
        {
            get { return trueParts.Count; }
        }

        /// <summary>
        /// Gets the count of the parts which are evaluated as <c>false</c>.
        /// </summary>
        public int FalsePartsCount
        {
            get { return falseParts.Count; }
        }

        /// <summary>
        /// Gets the count of the parts which can't be evaluated.
        /// </summary>
        public int UnknownPartsCount
        {
            get { return unknownParts.Count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionParts"/> class.
        /// </summary>
        /// <param name="condition">The assumed condition.</param>
        /// <param name="expressionParts">Evaluated values for condition parts.</param>
        /// <param name="flowOutputSet">Output set where condition will be assumed.</param>
        /// <remarks>Condition parts count should be aqual to expression parts count.</remarks>
        public ConditionParts(AssumptionCondition condition, IList<MemoryEntry> expressionParts, FlowOutputSet flowOutputSet)
        {
            Debug.Assert(condition.Parts.Count() == expressionParts.Count);

            this.flowOutputSet = flowOutputSet;
            conditionForm = condition.Form;

            var conditionParts = condition.Parts.ToArray();
            for (int i = 0; i < conditionParts.Length; i++)
            {
                ConditionPart part = new ConditionPart(conditionParts[i], expressionParts[i]);
                var conditionResult = part.GetConditionResult();
                if (conditionResult == ConditionPart.PossibleValues.OnlyFalse)
                {
                    falseParts.Add(part);
                }
                else if (conditionResult == ConditionPart.PossibleValues.OnlyTrue)
                {
                    trueParts.Add(part);
                }
                else
                {
                    unknownParts.Add(part);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to confirm the assumption and setup the environment inside of the assumed block.
        /// </summary>
        /// <returns><c>false</c> is returned if the assumption can be proved to be wrong; otherwise <c>true</c> is returned.</returns>
        public bool MakeAssumption()
        {
            //False parts can be used to exclude some values of the variables used in it. - The precision will be low.
            //True parts are the most precise for assuming the value of the variables used.
            //For the unknown parts, we assume that the condition is true. That will be used for narrowing down possible values of the variable used.

            bool willAssume;
            switch (conditionForm)
            {
                case ConditionForm.All:
                    willAssume = FalsePartsCount == 0;
                    break;
                case ConditionForm.None:
                    willAssume = TruePartsCount == 0;
                    break;
                case ConditionForm.Some:
                    willAssume = FalsePartsCount > 0;
                    break;
                case ConditionForm.SomeNot:
                    willAssume = FalsePartsCount > 0;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Condition form \"{0}\" is not supported", conditionForm));
            }

            if (willAssume)
            {
                AssumeTrue();
                AssumeUnknown();
                AssumeUnknown();
            }

            return willAssume;
        }

        #endregion

        #region Private methods

        void AssumeTrue()
        {

        }

        void AssumeUnknown()
        {
            // only variables which are not used in true parts will be evaluated.
        }

        void AssumeFalse()
        {
            // will be used only if the variable is not used in true or unknown parts. Possible values of the variable can be still infinit after few are eliminated.
        }

        #endregion
    }
}
