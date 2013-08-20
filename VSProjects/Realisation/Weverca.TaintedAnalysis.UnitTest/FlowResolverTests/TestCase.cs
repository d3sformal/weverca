using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;
using MemoryModel = Weverca.MemoryModels.MemoryModel;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    class TestCase
    {
        #region Enum

        public enum ConditionResults { True, False, Unkwnown };

        #endregion

        #region Members

        List<MemoryEntry> conditionsEvaluations = new List<MemoryEntry>();
        Expression[] expressions;
        List<TestCaseResult> results = new List<TestCaseResult>();

        #endregion

        #region Constructor

        public TestCase(Expression[] expressions, ConditionResults[] conditionResults)
        {
            this.expressions = expressions;

            foreach (var conditionResult in conditionResults)
            {
                if (conditionResult == ConditionResults.True)
                {
                    conditionsEvaluations.Add(new MemoryEntry(new BooleanValue(true)));
                }
                else if (conditionResult == ConditionResults.False)
                {
                    conditionsEvaluations.Add(new MemoryEntry(new BooleanValue(false)));
                }
                else if (conditionResult == ConditionResults.Unkwnown)
                {
                    conditionsEvaluations.Add(new MemoryEntry(new BooleanValue(true), new BooleanValue(false)));
                }
            }
        }

        #endregion

        #region Methods

        public TestCaseResult AddResult(ConditionForm conditionForm, bool assume)
        {
            var testCaseResult = new TestCaseResult(conditionForm, assume);
            results.Add(testCaseResult);
            return testCaseResult;
        }

        public void Run()
        {
            if (results.Count == 0)
            {
                throw new NotSupportedException("Could not run test with no result.");
            }

            foreach (var testResult in results)
            {
                var conditions = new AssumptionCondition(testResult.ConditionForm, expressions);

                var snapshot = new MemoryModel.Snapshot();
                snapshot.StartTransaction();
                FlowOutputSet flowOutputSet = new FlowOutputSet(snapshot);

                FlowResolver.FlowResolver flowResolver = new FlowResolver.FlowResolver();

                //This change is because of new API for retrieving values
                var log = new EvaluationLog();
                var index = 0;
                foreach (var part in conditions.Parts)
                {
                    log.AssociateValue(part.SourceElement, conditionsEvaluations[index]);
                }

                bool assume = flowResolver.ConfirmAssumption(flowOutputSet, conditions, log);

                Assert.AreEqual(testResult.Assume, assume);

                if (assume)
                {
                    snapshot.CommitTransaction();

                    testResult.ConfirmResults(flowOutputSet);
                }
            }
        }

        #endregion
    }
}
