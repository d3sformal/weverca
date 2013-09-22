using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core.AST;
using Weverca.Analysis;
using Weverca.Analysis.Expressions;

namespace Weverca.TaintedAnalysis.FlowResolver
{
    /// <summary>
    /// This class holds context of full condition and realise the assumptions.
    /// </summary>
    class ConditionParts
    {
        #region Members

        List<ConditionPart> conditionParts = new List<ConditionPart>();
        //TODO: something for holding values of variables. It might be useful to extend memory model.

        FlowOutputSet flowOutputSet;
        ConditionForm conditionForm;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the count of the parts which are evaluated as <c>true</c>.
        /// </summary>
        int TruePartsCount
        {
            get { return conditionParts.Where(c => c.ConditionResult == ConditionPart.PossibleValues.OnlyTrue).Count(); }
        }

        /// <summary>
        /// Gets the count of the parts which are evaluated as <c>false</c>.
        /// </summary>
        int FalsePartsCount
        {
            get { return conditionParts.Where(c => c.ConditionResult == ConditionPart.PossibleValues.OnlyFalse).Count(); }
        }

        /// <summary>
        /// Gets the count of the parts which can't be evaluated.
        /// </summary>
        int UnknownPartsCount
        {
            get { return conditionParts.Where(c => c.ConditionResult == ConditionPart.PossibleValues.Unknown).Count(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionParts"/> class.
        /// </summary>
        /// <param name="conditionForm">The condition form.</param>
        /// <param name="flowOutputSet">Output set where condition will be assumed.</param>
        /// <param name="log">The log of evaluation of the conditions' parts.</param>
        /// <param name="langElements">The elements of the condition.</param>
        public ConditionParts(ConditionForm conditionForm, FlowOutputSet flowOutputSet, EvaluationLog log, params LangElement[] langElements)
            : this(conditionForm, flowOutputSet, langElements.Select(a => new ConditionPart(a, log)))
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionParts" /> class.
        /// </summary>
        /// <param name="conditionForm">The condition form.</param>
        /// <param name="flowOutputSet">Output set where condition will be assumed.</param>
        /// <param name="log">The log of evaluation of the conditions' parts.</param>
        /// <param name="conditionParts">The elements of the condition.</param>
        public ConditionParts(ConditionForm conditionForm, FlowOutputSet flowOutputSet, EvaluationLog log, IEnumerable<Postfix> conditionParts)
            : this(conditionForm, flowOutputSet, log, conditionParts.Select(a => a.SourceElement).ToArray())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionParts" /> class.
        /// </summary>
        /// <param name="conditionForm">The condition form.</param>
        /// <param name="flowOutputSet">Output set where condition will be assumed.</param>
        /// <param name="conditionParts">The elements of the condition.</param>
        public ConditionParts(ConditionForm conditionForm, FlowOutputSet flowOutputSet, IEnumerable<ConditionPart> conditionParts)
        {
            this.flowOutputSet = flowOutputSet;
            this.conditionForm = conditionForm;
            this.conditionParts.AddRange(conditionParts);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to confirm the assumption and setup the environment inside of the assumed block.
        /// </summary>
        /// <returns><c>false</c> is returned if the assumption can be proved to be wrong; otherwise <c>true</c> is returned.</returns>
        public bool MakeAssumption()
        {
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
                    willAssume = FalsePartsCount > 0 || UnknownPartsCount > 0;
                    break;
                case ConditionForm.SomeNot:
                    willAssume = FalsePartsCount > 0 || UnknownPartsCount > 0;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Condition form \"{0}\" is not supported", conditionForm));
            }

            if (willAssume)
            {
                foreach (var conditionPart in conditionParts)
                {
                    conditionPart.AssumeCondition(flowOutputSet);
                }
            }

            return willAssume;
        }

        #endregion
    }
}
