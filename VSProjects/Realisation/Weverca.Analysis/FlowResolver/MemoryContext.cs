using System;
using System.Collections.Generic;
using System.Linq;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.ExpressionEvaluator;
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
        FlowOutputSet valueFactory;

        Dictionary<VariableName, List<Value>> variableValues = new Dictionary<VariableName, List<Value>>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryContext"/> class.
        /// </summary>
        /// <param name="log">Evaluation log used for evaluating variables which are not yet in the memoryContext.</param>
        /// <param name="valueFactory">ValueFactory used for creating value classes instances.</param>
        public MemoryContext(EvaluationLog log, FlowOutputSet valueFactory)
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
                    variableValues[variable.Key].AddRange(variable.Value);
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
                    MemoryEntry memoryEntry = variableInfo.ReadMemory(valueFactory.Snapshot);
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
                values.Add(newValue);
            }
            else if (newValue is IntervalValue)
            {
                //if there is already a concrete value, there is nothing to add, because the present conrete value is more precise.
                if (values.Any(a => a is ConcreteValue))
                {
                    return;
                }

                //if there are some other intarvalu values, we must delete those, which doesn't intersect the new one. We will add the intersection of those, which intersects the new one and the new one
                IntervalValue intersection = IntersectIntervals((IntervalValue)newValue, values.Where(a => a is IntervalValue).Select(a => (IntervalValue)a));

                //we will also delete more general values (any value, anystring, anyint, ...) also the intervals are already counted in.
                values.Clear();
                values.Add(intersection);
            }
            else if (newValue is AnyValue && values.Count > 0)
            {
                //There already is something, which is defitinelly more precise... or with the same precision. -- there is nothing to add
            }
            else
            {
                // in this case we are adding something more precise than anyValue.
                values.RemoveAll(a => a is AnyValue);
                values.Add(newValue);
            }
        }

        IntervalValue IntersectIntervals(IntervalValue interval, IEnumerable<IntervalValue> intervals)
        {
            if (intervals == null || intervals.Count() == 0)
            {
                return interval;
            }

            bool hasLong = intervals.Any(a => a is LongintIntervalValue);
            bool hasFloat = intervals.Any(a => a is FloatIntervalValue);
            bool hasInt = intervals.Any(a => a is IntegerIntervalValue);

            if (hasFloat || interval is FloatIntervalValue)
            {
                return IntersectIntervals(TypeConversion.ToFloatInterval(valueFactory, interval), intervals.Select(a => TypeConversion.ToFloatInterval(valueFactory, a)));
            }
            else if (hasLong || interval is LongintIntervalValue)
            {
                return IntersectIntervals(TypeConversion.ToLongInterval(valueFactory, interval), intervals.Select(a => TypeConversion.ToLongInterval(valueFactory, a)));
            }
            else if (hasInt || interval is IntegerIntervalValue)
            {
                return IntersectIntervals(TypeConversion.ToIntegerInterval(valueFactory, interval), intervals.Select(a => TypeConversion.ToIntegerInterval(valueFactory, a)));
            }
            else
            {
                throw new NotSupportedException(string.Format("Interval type \"{0}\" is not supported.", interval.GetType()));
            }
        }

        IntervalValue IntersectIntervals<T>(IntervalValue<T> interval, IEnumerable<IntervalValue<T>> intervals)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            IntervalValue<T> result = interval;
            
            T maxStart = result.Start;
            T minEnd = result.End;

            foreach (var item in intervals)
            {
                if (maxStart.CompareTo(item.Start) < 0)
                {
                    maxStart = item.Start;
                }

                if (minEnd.CompareTo(item.End) > 0)
                {
                    minEnd = item.End;
                }
            }

            if (maxStart is int)
            {
                return valueFactory.CreateIntegerInterval(System.Convert.ToInt32(maxStart), System.Convert.ToInt32(minEnd));
            }
            else if (maxStart is long)
            {
                return valueFactory.CreateLongintInterval(System.Convert.ToInt64(maxStart), System.Convert.ToInt64(minEnd));
            }
            if (maxStart is double)
            {
                return valueFactory.CreateFloatInterval(System.Convert.ToDouble(maxStart), System.Convert.ToDouble(minEnd));
            }
            else
            {
                throw new NotSupportedException(string.Format("Interval type \"{0}\" is not supported.", interval.GetType()));
            }
        }

        #endregion
    }
}
