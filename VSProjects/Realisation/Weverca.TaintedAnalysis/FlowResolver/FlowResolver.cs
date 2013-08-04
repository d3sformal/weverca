using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;
using Weverca.Analysis;
using Weverca.Analysis.Expressions;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.FlowResolver
{
    /// <summary>
    /// This class is used for evaluating conditions and assumptions.
    /// According to the result of the assumption the environment inside of the code block is set up.
    /// </summary>
    class FlowResolver : FlowResolverBase
    {
        #region Members

        private FlowOutputSet outSet;

        #endregion

        #region FlowResolverBase overrides

        /// <summary>
        /// Represents method which is used for confirming assumption condition. Assumption can be declined - it means that we can prove, that condition CANNOT be ever satisfied.
        /// </summary>
        /// <param name="outSet">Output set where condition will be assumed</param>
        /// <param name="condition">Assumed condition</param>
        /// <param name="expressionParts">Evaluated values for condition parts</param>
        /// <returns>
        /// <c>false</c> if condition cannot be ever satisfied, true otherwise.
        /// </returns>
        public override bool ConfirmAssumption(FlowOutputSet outSet, AssumptionCondition condition, MemoryEntry[] expressionParts)
        {
            //TODO: How to resolve not-bool conditions, like if (1) etc.?
            //TODO: if(False) there is empty avaluated parts --> is evaluated like "can be true".

            Debug.Assert(condition.Parts.Count() == expressionParts.Length);

            List<ConditionPart> trueParts = new List<ConditionPart>();
            List<ConditionPart> falseParts = new List<ConditionPart>();
            List<ConditionPart> unkwownParts = new List<ConditionPart>();

            //create and sort out condition parts according to the results of the conditions.
            int i = 0;
            foreach (var conditionPart in condition.Parts)
            {
                ConditionPart part = new ConditionPart(conditionPart, expressionParts[i]);
                i++;

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
                    unkwownParts.Add(part);
                }
            }

            this.outSet = outSet;

            bool willAssume;
            switch (condition.Form)
            {
                case ConditionForm.All:
                    willAssume = falseParts.Count == 0;
                    break;
                case ConditionForm.None:
                    willAssume = trueParts.Count == 0;
                    break;
                case ConditionForm.Some:
                    willAssume = trueParts.Count > 0;
                    break;
                case ConditionForm.SomeNot:
                    willAssume = falseParts.Count > 0;
                    break;
                default:
                    throw new NotSupportedException(string.Format("Condition form \"{0}\" is not supported", condition.Form));
            }

            //if (willAssume)
            //{
            //    ProcessAssumption(condition, expressionParts);
            //}

            return willAssume;
        }

        /// <summary>
        /// Is called after each invoked call - has to merge data from dispatched calls into callerOutput
        /// </summary>
        /// <param name="callerOutput">Output of caller, which dispatch calls</param>
        /// <param name="dispatchedProgramPointGraphs">Program point graphs obtained during analysis</param>
        /// <param name="callType">Type of merged call</param>
        public override void CallDispatchMerge(FlowOutputSet callerOutput, ProgramPointGraph[] dispatchedProgramPointGraphs,CallType callType)
        {
            var ends = dispatchedProgramPointGraphs.Select(c => c.End.OutSet as ISnapshotReadonly).ToArray();
            callerOutput.MergeWithCallLevel(ends);
        }

        /// <summary>
        /// Is called after each include/require/include_once/require_once expression (can be resolved according to flow.CurrentPartial)
        /// </summary>
        /// <param name="flow">Flow controller where include extensions can be stored</param>
        /// <param name="includeFile">File argument of include statement</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Include(FlowController flow, MemoryEntry includeFile)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Assume valid condition into output set
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="expressionParts"></param>
        void ProcessAssumption(AssumptionCondition condition, MemoryEntry[] expressionParts)
        {
            if (condition.Form == ConditionForm.All)
            {
                if (condition.Parts.Count() == 1)
                {
                    AssumeBinary(condition.Parts.First().SourceElement as BinaryEx, expressionParts[0]);
                }
            }
        }

        void AssumeBinary(BinaryEx exp, MemoryEntry expResult)
        {
            if (exp == null)
            {
                return;
            }
            switch (exp.PublicOperation)
            {
                case Operations.Equal:
                    AssumeEqual(exp.LeftExpr, exp.RightExpr, expResult);
                    break;
            }
        }

        void AssumeEqual(LangElement left, LangElement right, MemoryEntry result)
        {
            var leftVar = left as DirectVarUse;
            var rightVal = right as StringLiteral;

            if (leftVar == null)
            {
                //for simplicity resolve only $var==stringliteral statements
                return;
            }

            outSet.Assign(leftVar.VarName, outSet.CreateString(rightVal.Value as string));
        }

        #endregion
    }
}
