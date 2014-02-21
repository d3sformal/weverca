using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.FlowResolver
{
    /// <summary>
    /// This class holds context of full condition and realise the assumptions.
    /// </summary>
    class ConditionParts
    {
        #region Members

        List<ConditionPart> conditionParts = new List<ConditionPart>();

        FlowOutputSet flowOutputSet;
        ConditionForm conditionForm;
        EvaluationLog log;

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
        /// Initializes a new instance of the <see cref="ConditionParts" /> class.
        /// </summary>
        /// <param name="conditionForm">The condition form.</param>
        /// <param name="flowOutputSet">Output set where condition will be assumed.</param>
        /// <param name="log">The log of evaluation of the conditions' parts.</param>
        /// <param name="langElements">The elements of the condition.</param>
        public ConditionParts(ConditionForm conditionForm, FlowOutputSet flowOutputSet, EvaluationLog log, params LangElement[] langElements)
            : this(conditionForm, flowOutputSet, log, langElements.Select(a => new ConditionPart(a, log, flowOutputSet)))
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
        /// <param name="log">The log of evaluation of the conditions' parts.</param>
        /// <param name="conditionParts">The elements of the condition.</param>
        public ConditionParts(ConditionForm conditionForm, FlowOutputSet flowOutputSet, EvaluationLog log, IEnumerable<ConditionPart> conditionParts)
        {
            this.flowOutputSet = flowOutputSet;
            this.conditionParts.AddRange(conditionParts);
            this.log = log;
            
            this.conditionForm = conditionForm;
            if (this.conditionParts.Count == 1)
            {
                if (conditionForm == ConditionForm.Some || conditionForm == ConditionForm.ExactlyOne)
                {
                    this.conditionForm = ConditionForm.All;
                }
                else if (conditionForm == ConditionForm.SomeNot || conditionForm == ConditionForm.NotExactlyOne)
                {
                    this.conditionForm = ConditionForm.None;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to confirm the assumption and setup the environment inside of the assumed block.
        /// </summary>
        /// <param name="outputMemoryContext">
        ///     If set to <c>null</c>, output will be written to flowOutputSet given provided while constracting this instance;
        ///     otherwise output will be written into this parameter.
        /// </param>
        /// <returns>
        ///   <c>false</c> is returned if the assumption can be proved to be wrong; otherwise <c>true</c> is returned.
        /// </returns>
        public bool MakeAssumption(MemoryContext outputMemoryContext)
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
                    willAssume = TruePartsCount > 0 || UnknownPartsCount > 0;
                    break;
                case ConditionForm.SomeNot:
                    willAssume = FalsePartsCount > 0 || UnknownPartsCount > 0;
                    break;
                case ConditionForm.ExactlyOne:
                    willAssume = TruePartsCount == 1 || UnknownPartsCount > 0;
                    break;
                case ConditionForm.NotExactlyOne:
                    willAssume = TruePartsCount != 1 || UnknownPartsCount > 0;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Condition form \"{0}\" is not supported", conditionForm));
            }

            if (willAssume)
            {
                MemoryContext memoryContext = outputMemoryContext ?? new MemoryContext(log, flowOutputSet);
                
                bool intersectionMerge = conditionForm == ConditionForm.All || conditionForm == ConditionForm.None || conditionForm == ConditionForm.ExactlyOne ?
                    true : false;

                foreach (var conditionPart in conditionParts)
                {
                    MemoryContext currentMemoryContext = new MemoryContext(log, flowOutputSet);
                    conditionPart.AssumeCondition(conditionForm, currentMemoryContext, flowOutputSet);
                    if (intersectionMerge)
                    {
                        memoryContext.IntersectionMerge(currentMemoryContext);
                    }
                    else
                    {
                        memoryContext.UnionMerge(currentMemoryContext);
                    }
                }

                //If this condition is false, then we are in recursion. Made by splitting 1 condition with logic operator into two.
                if (outputMemoryContext == null)
                {
                    memoryContext.AssignToSnapshot(flowOutputSet.Snapshot);
                }
            }

            return willAssume;
        }

        #endregion
    }
}
