using System.Collections.Generic;
using System.Linq;
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
        public enum ConditionResults { True, False, Unkwnown };
        
        AssumptionCondition conditions;
        List<MemoryEntry> conditionsEvaluations = new List<MemoryEntry>();

        Dictionary<string, List<Value>> results = new Dictionary<string, List<Value>>();

        public TestCase(ConditionForm conditionForm, Expression[] expressions, ConditionResults[] conditionResults)
        {
            conditions = new AssumptionCondition(conditionForm, expressions);
            
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

        public void AddResult(string variableName, Value value)
        {
            if (!results.ContainsKey(variableName))
            {
                results.Add(variableName, new List<Value>());
            }
            results[variableName].Add(value);
        }

        public void Run(bool expectedAssume)
        {
            var snapshot = new MemoryModel.Snapshot();
            snapshot.StartTransaction();
            FlowOutputSet flowOutputSet = new FlowOutputSet(snapshot);

            FlowResolver.FlowResolver flowResolver = new FlowResolver.FlowResolver();

            //This change is because of new API for retrieving values
            var log = new EvaluationLog();
            var index=0;
            foreach (var part in conditions.Parts) {
                log.AssociateValue(part.SourceElement, conditionsEvaluations[index]);
            }

            bool assume = flowResolver.ConfirmAssumption(flowOutputSet, conditions, log);

            Assert.AreEqual(expectedAssume, assume);

            if (assume)
            {
                snapshot.CommitTransaction();

                ConfirmResults(flowOutputSet);
            }
        }

        void ConfirmResults(FlowOutputSet flowOutputSet)
        {
            foreach (var result in results)
            {
                var values = flowOutputSet.ReadValue(new VariableName(result.Key));

                ConfirmValues(result.Value.ToArray(), values.PossibleValues.ToArray());
            }
        }

        void ConfirmValues(Value[] expectedValues, Value[] values)
        {
            Assert.AreEqual(values.Length, expectedValues.Length);

            for (int i = 0; i < values.Length; i++)
            {
                ConfirmValue(expectedValues[i], values[i]);
            }
        }

        void ConfirmValue(Value expectedValue, Value value)
        {
            var visitor = new EqualsValueVisitor(expectedValue);
            value.Accept(visitor);
        }
    }
}
