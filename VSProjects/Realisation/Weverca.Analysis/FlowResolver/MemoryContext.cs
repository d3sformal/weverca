/*
Copyright (c) 2012-2014 Matyas Brenner and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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
                    foreach (var newValue in variable.Value)
                    {
                        IntersectValues(variableValues[variable.Key], newValue);
                    }
                }
            }
        }

        public void IntersectionAssign(VariableName variableName, LangElement element, IEnumerable<Value> values)
        {
            foreach (var value in values)
            {
                IntersectionAssign(variableName, element, value);
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
            AddValuesFromSnapshot(variableName, element);

            IntersectValues(variableValues[variableName], value);
        }
        /// <summary>
        /// Removes the undefined value from the list of possible values of the variable variableName.
        /// </summary>
        /// <param name='variableName'>
        /// The name of the variable which name will is removed.
        /// </param>
        /// <param name='element'>
        /// Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.
        /// </param>
        public void RemoveUndefinedValue(VariableName variableName, LangElement element) 
        {
            AddValuesFromSnapshot(variableName, element);

            variableValues[variableName].RemoveAll(a => a is UndefinedValue);
        }

        /// <summary>
        /// Assigns the undefined value to the variable variableName.
        /// </summary>
        /// <param name='variableName'>
        /// The name of the variable which is assigned.
        /// </param>
        public void AssignUndefinedValue(VariableName variableName) 
        {
            var undefList = new List<Value>();
            undefList.Add(valueFactory.UndefinedValue);
            if (!variableValues.ContainsKey(variableName))
            {
                variableValues.Add(variableName, undefList);
            } else 
            {
                variableValues[variableName] = undefList;
            }
        }

        /// <summary>
        /// Assigns the values which are evaluable as <c>true</c> to the variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="element">Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.</param>
        public void AssignTrueEvaluable(VariableName variableName, LangElement element)
        {
            AssignEvaluable(variableName, element, true);
        }

        /// <summary>
        /// Assigns the values which are evaluable as <c>false</c> to the variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="element">Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.</param>
        public void AssignFalseEvaluable(VariableName variableName, LangElement element)
        {
            AssignEvaluable(variableName, element, false);
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

        private void AddValuesFromSnapshot(VariableName variableName, LangElement element)
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
        }

        bool ToBoolean(SnapshotBase snapshot, Value value, bool defaultValue)
        {
            var converter = new BooleanConverter(snapshot);
            var result = converter.EvaluateToBoolean(value);
            if (result != null)
            {
                return result.Value;
            }
            else
            {
                return defaultValue;
            }
        }

        void AssignEvaluable(VariableName variableName, LangElement element, bool desiredEvaluation)
        {
            var variableInfo = log.ReadSnapshotEntry(element);
            if (variableInfo != null)
            {
                MemoryEntry memoryEntry = variableInfo.ReadMemory(valueFactory.Snapshot);
                if (memoryEntry != null && memoryEntry.PossibleValues != null)
                {
                    var possibleValues = memoryEntry.PossibleValues.Where(a => ToBoolean(valueFactory.Snapshot, a, desiredEvaluation) == desiredEvaluation);

                    if (!variableValues.ContainsKey(variableName))
                    {
                        variableValues.Add(variableName, possibleValues.ToList());
                    }
                    else
                    {
                        variableValues[variableName] = possibleValues.ToList();
                    }
                }
            }
        }

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
                FloatIntervalValue interval = TypeConversion.ToFloatInterval(valueFactory, (IntervalValue)newValue);
                if (interval != null)
                {
                    values.RemoveAll(a => ToRemove(a, interval));
                }
                
                //if there is already a concrete value, there is nothing to add, because the present conrete value is more precise.
                if (values.Any(a => a is ConcreteValue))
                {
                    return;
                }

                //if there are some other intarvalu values, we must delete those, which doesn't intersect the new one. We will add the intersection of those, which intersects the new one and the new one
                Value intersection = IntersectIntervals((IntervalValue)newValue, values.Where(a => a is IntervalValue).Select(a => (IntervalValue)a));

                //we will also delete more general values (any value, anystring, anyint, ...) also the intervals are already counted in.
                values.Clear();
                if (intersection != null)
                {
                    values.Add(intersection);
                }
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

        bool ToRemove(Value value, FloatIntervalValue interval)
        {
            var visitor = new ToFloatConversionVisitor(valueFactory);
            value.Accept(visitor);
            var floatValue = visitor.Result;

            if (floatValue != null)
            {
                return floatValue.Value < interval.Start || floatValue.Value > interval.End;
            }

            return false;
        }

        Value IntersectIntervals(IntervalValue interval, IEnumerable<IntervalValue> intervals)
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

        Value IntersectIntervals<T>(IntervalValue<T> interval, IEnumerable<IntervalValue<T>> intervals)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            T maxStart = interval.Start;
            T minEnd = interval.End;

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

            if (maxStart.Equals(minEnd))
            {
                if (maxStart is int)
                {
                    return valueFactory.CreateInt(System.Convert.ToInt32(maxStart));
                }
                else if (maxStart is long)
                {
                    return valueFactory.CreateLong(System.Convert.ToInt64(maxStart));
                }
                if (maxStart is double)
                {
                    return valueFactory.CreateDouble(System.Convert.ToDouble(maxStart));
                }
                else
                {
                    throw new NotSupportedException(string.Format("Interval type \"{0}\" is not supported.", interval.GetType()));
                }
            }
            if (maxStart.CompareTo(minEnd) < 0)
            {

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
            else
            {
                return valueFactory.UndefinedValue;
            }
        }

        #endregion
    }
}