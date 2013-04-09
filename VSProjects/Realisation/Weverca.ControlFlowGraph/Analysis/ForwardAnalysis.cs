using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PHP.Core.AST;

using Weverca.ControlFlowGraph.Analysis.Expressions;

namespace Weverca.ControlFlowGraph.Analysis
{
    /// <summary>
    /// Extending layer for forward analysis, that provides methods for expression analysing.
    /// </summary>
    /// <typeparam name="FlowInfo"></typeparam>
    public abstract class ForwardAnalysis<FlowInfo> : ForwardAnalysisAbstract<FlowInfo>
    {
        ExpressionWalker<FlowInfo> _services;


        public ForwardAnalysis(ControlFlowGraph entryMethod, ExpressionEvaluator<FlowInfo> evaluator)
            : base(entryMethod)
        {
            _services = new ExpressionWalker<FlowInfo>(evaluator);
        }

        protected override void FlowThrough(FlowInputSet<FlowInfo> inSet, LangElement statement, FlowOutputSet<FlowInfo> outSet)
        {
            //TODO push pop function calls handling
            outSet.FillFrom(inSet);
            var expression = convertExpression(statement);
            flowThrough(inSet, expression, outSet);
        }


        protected override bool ConfirmAssumption(FlowInputSet<FlowInfo> inSet, AssumptionCondition condition, FlowOutputSet<FlowInfo> outSet)
        {
            outSet.FillFrom(inSet);
            if (condition.Parts.Count() == 0)
            {
                return true;
            }

            var partResults = new List<FlowInfo>();

            foreach (var part in condition.Parts)
            {
                var expression = convertExpression(part);
                var partResult = flowThrough(inSet, expression, outSet);

                partResults.Add(partResult);
            }
            

            var result = initialFor(condition.Form);
            foreach (var res in partResults)
            {
                switch (condition.Form)
                {
                    case ConditionForm.SomeNot:
                        if (!canProveTrue(inSet, res))
                        {
                            result = true;
                        }
                        break;

                    case ConditionForm.All:
                        if (canProveFalse(inSet, res))
                            return false;
                        break;
                        
                    default:
                        throw new NotImplementedException();
                }
            }

            if (result)
            {
                Assume(inSet, condition, outSet);
            }
            return result;
        }

        /// <summary>
        /// prove that given conditionResult is always false
        /// </summary>
        /// <param name="inSet"></param>
        /// <param name="conditionResult"></param>
        /// <returns></returns>
        protected abstract bool canProveFalse(FlowInputSet<FlowInfo> inSet, FlowInfo conditionResult);

        /// <summary>
        /// prove that given conditionResult is always true
        /// </summary>
        /// <param name="inSet"></param>
        /// <param name="conditionResult"></param>
        /// <returns></returns>
        protected abstract bool canProveTrue(FlowInputSet<FlowInfo> inSet, FlowInfo conditionResult);

        protected virtual void Assume(FlowInputSet<FlowInfo> inSet, AssumptionCondition condition, FlowOutputSet<FlowInfo> outSet)
        {
            //by default we won't assume anything
        }

        protected override void BlockMerge(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override void IncludeMerge(IEnumerable<FlowInputSet<FlowInfo>> inSets, FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override void CallMerge(IEnumerable<FlowInputSet<FlowInfo>> inSets, FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }


        #region Private utils
        private static PostfixExpression convertExpression(LangElement statement)
        {
            var converter = new ExpressionConverter();
            statement.VisitMe(converter);
            var expression = converter.GetExpression();
            expression.Add(statement);
            return expression;
        }

        private FlowInfo flowThrough(FlowInputSet<FlowInfo> inSet, PostfixExpression expression, FlowOutputSet<FlowInfo> outSet)
        {
            _services.Clear();
            for (int i = 0; i < expression.Length; ++i)
            {
                var element = expression.GetElement(i);
                _services.Eval(inSet, element, outSet);
            }
            return _services.Pop();
        }

        private bool initialFor(ConditionForm form)
        {
            switch (form)
            {
                case ConditionForm.SomeNot:
                case ConditionForm.Some:
                    return false;
                case ConditionForm.All:
                case ConditionForm.None:
                    return true;
                default:
                    throw new NotSupportedException("Unsupported condition form");
            }
        }

        #endregion

    }
}
