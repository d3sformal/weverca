using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core;
using PHP.Core.AST;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.FlowResolver
{
    /// <summary>
    /// Class for holding memory context - variables and it's values.
    /// </summary>
    class MemoryContext
    {
        #region Members

        EvaluationLog log;
        SnapshotBase valueFactory;

        Dictionary<VariableName, List<Value>> variableValues = new Dictionary<VariableName, List<Value>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryContext"/> class.
        /// </summary>
        /// <param name="log">Evaluation log used for evaluating variables which are not yet in the memoryContext.</param>
        /// <param name="valueFactory">ValueFactory used for creating value classes instances.</param>
        public MemoryContext(EvaluationLog log, SnapshotBase valueFactory)
        {
            this.log = log;
            this.valueFactory = valueFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the variable values held in this class to the flowOutputSet.
        /// </summary>
        /// <param name="flowOutputSet">The flow output set where the values will be assigned.</param>
        public void AssignToSnapshot(SnapshotBase flowOutputSet)
        {
            foreach (var variableValue in variableValues)
            {
                var variableInfo = flowOutputSet.GetVariable(new VariableIdentifier(variableValue.Key));
                MemoryEntry values = new MemoryEntry(variableValue.Value.ToArray());
                variableInfo.WriteMemory(flowOutputSet, values);
            }
        }

        /// <summary>
        /// Merges other instance into this one.
        /// Values for the variables are being united.
        /// </summary>
        /// <param name="other">The memory context to merged.</param>
        public void UnionMerge(MemoryContext other)
        {
            foreach (var variable in other.variableValues)
            {
                if (!variableValues.ContainsKey(variable.Key))
                {
                    variableValues.Add(variable.Key, variable.Value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Merges other instance into this one.
        /// Values for the variables are being intersected.
        /// </summary>
        /// <param name="other">The memory context to merge.</param>
        public void IntersectionMerge(MemoryContext other)
        {
            foreach (var variable in other.variableValues)
            {
                if (!variableValues.ContainsKey(variable.Key))
                {
                    variableValues.Add(variable.Key, variable.Value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Assign a new value for a variable.
        /// New value will be intersected with values already registered.
        /// </summary>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="element">Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.</param>
        /// <param name="value">The value which will be assigned to the variable.</param>
        public void IntersectionAssign(VariableName variableName, LangElement element, Value value)
        {
            if (!variableValues.ContainsKey(variableName))
            {
                variableValues.Add(variableName, new List<Value>());
                var variableInfo = log.ReadSnapshotEntry(element);
                if (variableInfo != null)
                {
                    MemoryEntry memoryEntry = variableInfo.ReadMemory(valueFactory);
                    if (memoryEntry != null && memoryEntry.PossibleValues != null)
                    {
                        variableValues[variableName].AddRange(memoryEntry.PossibleValues);
                    }
                }
            }

            //TODO: Proper implementation. This is just a demo.
            variableValues[variableName].Add(value);
        }

        #endregion

        #region Factory Methods

        public UndefinedValue UndefinedValue { get { return valueFactory.UndefinedValue; } }

        public AnyStringValue AnyStringValue { get { return valueFactory.AnyStringValue; } }

        public StringValue CreateString(string literal)
        {
            return valueFactory.CreateString(literal);
        }

        public BooleanValue CreateBool(bool boolean)
        {
            return valueFactory.CreateBool(boolean);
        }

        public FloatValue CreateDouble(double number)
        {
            return valueFactory.CreateDouble(number);
        }

        public IntegerValue CreateInt(int number)
        {
            return valueFactory.CreateInt(number);
        }

        public LongintValue CreateLong(long number)
        {
            return valueFactory.CreateLong(number);
        }

        public LongintIntervalValue CreateLongintInterval(long start, long end)
        {
            return valueFactory.CreateLongintInterval(start, end);
        }

        public IntegerIntervalValue CreateIntegerInterval(int start, int end)
        {
            return valueFactory.CreateIntegerInterval(start, end);
        }

        public FloatIntervalValue CreateFloatInterval(double start, double end)
        {
            return valueFactory.CreateFloatInterval(start, end);
        }

        #endregion
    }
}
