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
        Stack<StatementWalker<FlowInfo>> _statementStack = new Stack<StatementWalker<FlowInfo>>();

        ExpressionEvaluator<FlowInfo> _evaluator;
        DeclarationResolver<FlowInfo> _resolver;


        public ForwardAnalysis(ControlFlowGraph entryMethod, ExpressionEvaluator<FlowInfo> evaluator, DeclarationResolver<FlowInfo> resolver)
            : base(entryMethod)
        {
            _evaluator = evaluator;
            _resolver = resolver;
        }

        protected override void FlowThrough(FlowInputSet<FlowInfo> inSet, LangElement statement, FlowOutputSet<FlowInfo> outSet)
        {


            var walker = tryPush(statement);
            if (walker.AtStart)
            {
                //at start fill out set from inSet
                outSet.FillFrom(inSet);
            }
            flowThrough(inSet, walker, outSet);
            tryPop();
        }


        protected override bool ConfirmAssumption(FlowInputSet<FlowInfo> inSet, AssumptionCondition condition, FlowOutputSet<FlowInfo> outSet)
        {
            outSet.FillFrom(inSet);
            if (!condition.Parts.Any())
            {
                return true;
            }

            var partResults = new List<FlowInfo>();

            foreach (var part in condition.Parts)
            {
                var walker = tryPush(part);
                flowThrough(inSet, walker, outSet);
                tryPop();

                if (!walker.IsComplete)
                {
                    throw new NotImplementedException("Dispatch during assumption is not implemented");
                }

                partResults.Add(walker.Result);
            }


            bool result;
            var canProve = proveAssumptionCondition(inSet, partResults, condition.Form, out result);

            if (!canProve)
                result = true;

            if (result)
            {
                Assume(inSet, condition, outSet);
            }
            return result;
        }

        private bool proveAssumptionCondition(FlowInputSet<FlowInfo> inSet, List<FlowInfo> partResults, ConditionForm form, out bool result)
        {
            result = initialFor(form);
            foreach (var res in partResults)
            {
                switch (form)
                {
                    case ConditionForm.SomeNot:
                        if (!canProveTrue(inSet, res))
                        {
                            result = true;
                            return true;
                        }
                        break;

                    case ConditionForm.All:
                        if (canProveFalse(inSet, res))
                        {
                            result = false;
                            return true;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }
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


        protected abstract FlowInfo extractReturnValue(FlowInputSet<FlowInfo> callOutput);

        protected virtual void Assume(FlowInputSet<FlowInfo> inSet, AssumptionCondition condition, FlowOutputSet<FlowInfo> outSet)
        {
            //by default we won't assume anything
        }

        protected override void CallMerge(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet)
        {
            BlockMerge(inSet1, inSet2, outSet);
        }

        protected override void BlockMerge(FlowInputSet<FlowInfo> inSet1, FlowInputSet<FlowInfo> inSet2, FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override void IncludeMerge(IEnumerable<FlowInputSet<FlowInfo>> inSets, FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }

        protected override void ReturnedFromCall(FlowInputSet<FlowInfo> callerInSet, FlowInputSet<FlowInfo> callOutput, FlowOutputSet<FlowInfo> outSet)
        {
            throw new NotImplementedException();
        }


        #region Private utils

        private StatementWalker<FlowInfo> tryPush(LangElement statement)
        {
            if (LastCallOutput != null)
            {
                //there is call output for current walker
                return _statementStack.Peek();
            }

            //push new walker
            var expr = convertExpression(statement);
            var walker = new StatementWalker<FlowInfo>(expr, _evaluator, _resolver);
            _statementStack.Push(walker);
            return walker;
        }

        private void tryPop()
        {
            if (_statementStack.Peek().IsComplete)
            {
                _statementStack.Pop();
            }
        }

        private void flowThrough(FlowInputSet<FlowInfo> inSet, StatementWalker<FlowInfo> walker, FlowOutputSet<FlowInfo> outSet)
        {
            if (walker.AwaitingCallReturn)
            {
                var returnValue = extractReturnValue(LastCallOutput);
                walker.InsertCallReturn(returnValue);
            }

            while (walker.CanEvalNext)
            {
                walker.EvalNext(inSet, outSet);
            }
        }


        private static PostfixExpression convertExpression(LangElement statement)
        {
            var converter = new ExpressionConverter();
            statement.VisitMe(converter);
            var expression = converter.GetExpression();
            expression.Add(statement);
            return expression;
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
