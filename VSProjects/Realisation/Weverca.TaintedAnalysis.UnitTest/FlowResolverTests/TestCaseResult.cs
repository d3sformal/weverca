using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using PHP.Core;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    enum ConditionResults { True, False, Unkwnown };

    class TestCaseResult
    {
        #region Members

        Dictionary<string, List<Value>> results = new Dictionary<string, List<Value>>();

        #endregion

        #region Properties

        public ConditionForm ConditionForm { get; private set; }
        public bool Assume { get; private set; }
        public List<MemoryEntry> ConditionsEvaluations { get; private set; }

        #endregion

        #region Constructor

        public TestCaseResult(ConditionForm conditionForm, bool assume, ConditionResults[] conditionResults)
        {
            ConditionForm = conditionForm;
            Assume = assume;

            ConditionsEvaluations = new List<MemoryEntry>();
            foreach (var conditionResult in conditionResults)
            {
                if (conditionResult == ConditionResults.True)
                {
                    ConditionsEvaluations.Add(new MemoryEntry(new BooleanValue(true)));
                }
                else if (conditionResult == ConditionResults.False)
                {
                    ConditionsEvaluations.Add(new MemoryEntry(new BooleanValue(false)));
                }
                else if (conditionResult == ConditionResults.Unkwnown)
                {
                    ConditionsEvaluations.Add(new MemoryEntry(new BooleanValue(true), new BooleanValue(false)));
                }
            }
        }

        #endregion

        #region Methods

        public TestCaseResult AddResultValue(string variableName, Value value)
        {
            if (!results.ContainsKey(variableName))
            {
                results.Add(variableName, new List<Value>());
            }
            results[variableName].Add(value);

            return this;
        }

        public void ConfirmResults(FlowOutputSet flowOutputSet)
        {
            foreach (var result in results)
            {
                var values = flowOutputSet.ReadValue(new VariableName(result.Key));

                ConfirmValues(result.Value.ToArray(), values.PossibleValues.ToArray());
            }
        }

        #endregion

        #region Private methods

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

        #endregion
    }
}
