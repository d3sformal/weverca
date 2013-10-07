using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;
using MemoryModel = Weverca.MemoryModels.MemoryModel;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    class TestCase
    {
        #region Members

        Expression[] expressions;
        List<TestCaseResult> results = new List<TestCaseResult>();
        Dictionary<Expression, Value> associations = new Dictionary<Expression, Value>();
        Dictionary<string, Value> variableAssociations = new Dictionary<string, Value>();

        #endregion

        #region Constructor

        public TestCase(params Expression[] expressions)
        {
            this.expressions = expressions;
        }

        public static TestCase Create(params Expression[] expressions)
        {
            return new TestCase(expressions);
        }

        #endregion

        #region Methods

        public TestCaseResult AddResult(ConditionForm conditionForm, bool assume, params ConditionResults[] conditionResults)
        {
            var testCaseResult = new TestCaseResult(this, conditionForm, assume, conditionResults);
            results.Add(testCaseResult);
            return testCaseResult;
        }

        public TestCase Associate(Expression expression, Value value)
        {
            associations.Add(expression, value);
            return this;
        }

        public TestCase AssociateVariable(string variableName, Value value)
        {
            variableAssociations.Add(variableName, value);
            return this;
        }

        public void Run()
        {
            if (results.Count == 0)
            {
                throw new NotSupportedException("Could not run test with no result.");
            }

            foreach (var testResult in results)
            {
                AssumptionCondition conditions = new AssumptionCondition(testResult.ConditionForm, expressions);

                var snapshot = new MemoryModel.Snapshot();
                snapshot.StartTransaction();
                FlowOutputSet flowOutputSet = new FlowOutputSet(snapshot);

                FlowResolver.FlowResolver flowResolver = new FlowResolver.FlowResolver();
                foreach (var association in variableAssociations)
                {
                    flowOutputSet.Assign(new VariableName(association.Key), association.Value);
                }

                //This change is because of new API for retrieving values
                EvaluationLog log = new EvaluationLog();
                int index = 0;
                foreach (var part in conditions.Parts)
                {
                    log.AssociateValue(part.SourceElement, testResult.ConditionsEvaluations[index]);
                }
                foreach (var association in associations)
                {
                    log.AssociateValue(association.Key, new MemoryEntry(association.Value));
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
