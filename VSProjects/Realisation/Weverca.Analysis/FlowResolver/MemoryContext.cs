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

            IntersectValues(variableValues[variableName], value);
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

        #region private methods

        void IntersectValues(List<Value> values, Value newValue)
        {
            if (newValue is ConcreteValue)
            {
                //There is nothin more precise, so we can delete anything.
                //If there is a precise value already present, the newValue should be one of them.
                values.Clear();
            }
            else if (newValue is IntervalValue)
            {
                //if there is already a concrete value, there is nothing to add, because the present conrete value is more precise.
                if (values.Any(a => a is ConcreteValue))
                {
                    return;
                }

                //if there are some other intarvalu values, we must delete those, which doesn't intersect the new one. We will add the intersection of those, which intersects the new one and the new one

                //we will also delete more general values (any value, anystring, anyint, ...)
            }
            else if (newValue is AnyValue && values.Count > 0)
            {
                //There already is something, which is defitinelly more precise... or with the same precision.
                return;
            }
            else
            {
                // in this case we are adding something more precise than anyValue.
                values.RemoveAll(a => a is AnyValue);
            }

            values.Add(newValue);
        }

        #endregion
    }
}
