/*
Copyright (c) 2012-2014 David Hauzar and Matyas Brenner

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

        public void IntersectionAssign(LangElement element, IEnumerable<Value> values)
        {
            foreach (var value in values)
            {
                IntersectionAssign(element, value);
            }
        }

        public void IntersectionAssign(LangElement first, LangElement second)
        {
            var variableInfoFirst = log.GetSnapshotEntry(first);
            if (variableInfoFirst == null) return;
            var memoryEntryFirst = variableInfoFirst.ReadMemory(valueFactory.Snapshot);
            if (memoryEntryFirst == null) return;

            var variableInfoSecond = log.ReadSnapshotEntry(second);
            if (variableInfoSecond == null) return;
            var memoryEntrySecond= variableInfoSecond.ReadMemory(valueFactory.Snapshot);
            if (memoryEntrySecond == null) return;
            
            var elementValues = new List<Value>(memoryEntryFirst.PossibleValues);

            elementValues = IntersectValues(elementValues, memoryEntrySecond.PossibleValues);

            variableInfoFirst.WriteMemory(valueFactory.Snapshot, new MemoryEntry(elementValues.ToArray()));
            var variableInfoSecondWrite = log.GetSnapshotEntry(second);
            if (variableInfoSecondWrite != null)
                variableInfoSecondWrite.WriteMemory(valueFactory.Snapshot, new MemoryEntry(elementValues.ToArray()));

        }

        /// <summary>
        /// Assign a new value for a variable.
        /// New value will be intersected with values already registered.
        /// </summary>
        /// <param name="variable">The name of the variable.</param>
        /// <param name="element">Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.</param>
        /// <param name="value">The value which will be assigned to the variable.</param>
        public void IntersectionAssign(LangElement element, Value value)
        {
            var variableInfo = log.GetSnapshotEntry(element);
            if (variableInfo == null) return;
            var memoryEntry = variableInfo.ReadMemory(valueFactory.Snapshot);
            if (memoryEntry == null || memoryEntry.PossibleValues == null) return;
            var elementValues = new List<Value>(memoryEntry.PossibleValues);

            IntersectValues(elementValues, value);
            //write to memory entry new values
            variableInfo.WriteMemory(valueFactory.Snapshot, new MemoryEntry(elementValues.ToArray()));
        }

        /// <summary>
        /// Removes the undefined value from the list of possible values of the element.
        /// </summary>
        /// <param name='element'>
        /// The AST element which values are restricted - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.
        /// </param>
        public void RemoveUndefinedValue(LangElement element)
        {
            var variableInfo = log.GetSnapshotEntry(element);

            if (variableInfo != null)
            {
                var values = GetValues(variableInfo);
                if (values != null)
                {
                    values.RemoveAll(a => a is UndefinedValue);
                    variableInfo.WriteMemory(valueFactory.Snapshot, new MemoryEntry(values.ToArray()));
                }
            }
        }

        /// <summary>
        /// Assigns the undefined value to the AST element element.
        /// </summary>
        /// <param name='element'>
        /// The AST element which values are restricted - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.
        /// </param>
        public void AssignUndefinedValue(LangElement element)
        {
            var variableInfo = log.GetSnapshotEntry(element);

            if (variableInfo != null)
            {
                variableInfo.WriteMemory(valueFactory.Snapshot, new MemoryEntry(valueFactory.UndefinedValue));
            }
        }


        private List<Value> GetValues(ReadSnapshotEntryBase variable)
        {
            MemoryEntry memoryEntry = variable.ReadMemory(valueFactory.Snapshot);
            if (memoryEntry != null && memoryEntry.PossibleValues != null)
            {
                var values = new List<Value>(memoryEntry.PossibleValues.Count());
                values.AddRange(memoryEntry.PossibleValues);
                return values;
            }

            return null;
        }

        /// <summary>
        /// Assigns the values which are evaluable as <c>true</c> to the variable.
        /// </summary>
        /// <param name="element">Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.</param>
        public void AssignTrueEvaluable(LangElement element)
        {
            AssignEvaluable(element, true);
        }

        /// <summary>
        /// Assigns the values which are evaluable as <c>true</c> to the variable.
        /// </summary>
        /// <param name="element">Language element from which is the variable being accessed - Used for gaining original evaluation of the variable via <see cref="EvaluationLog"/>.</param>
        public void AssignFalseEvaluable(LangElement element)
        {
            AssignEvaluable(element, false);
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

        void AssignEvaluable(LangElement element, bool desiredEvaluation)
        {
            var variableInfo = log.GetSnapshotEntry(element);
            if (variableInfo != null)
            {
                MemoryEntry memoryEntry = variableInfo.ReadMemory(valueFactory.Snapshot);
                if (memoryEntry != null && memoryEntry.PossibleValues != null)
                {
                    var possibleValues = memoryEntry.PossibleValues.Where(a => ToBoolean(valueFactory.Snapshot, a, desiredEvaluation) == desiredEvaluation);
                    var possibleValuesUnique = new HashSet<Value>(possibleValues);
                    var isAnyBooleanValue = memoryEntry.PossibleValues.Any(a => a is AnyBooleanValue);
                    if (isAnyBooleanValue)
                    {
                        possibleValuesUnique.Add(valueFactory.CreateBool(desiredEvaluation));
                    }

                    variableInfo.WriteMemory(valueFactory.Snapshot, new MemoryEntry(possibleValuesUnique));
                    var temp = variableInfo.ReadMemory(valueFactory.Snapshot);
                }
            }
        }

        List<Value> IntersectValues(IEnumerable<Value> leftValues, IEnumerable<Value> rightValues)
        {
            var result = new List<Value>();

            // AnyValue on left or right side
            var leftAny = leftValues.Any(a => a.GetType() == typeof(AnyValue));
            var rightAny = rightValues.Any(a => a.GetType() == typeof(AnyValue));

            if (leftAny && rightAny)
            {
                result.Add(valueFactory.Snapshot.AnyValue);
                return result;
            }

            if (leftAny)
            {
                result.AddRange(rightValues);
                return result;
            }

            if (rightAny)
            {
                result.AddRange(leftValues);
                return result;
            }


            var leftAnyValues = leftValues.Where(a => a is AnyValue);
            var rightAnyValues = leftValues.Where(a => a is AnyValue);
            

            // Intersect boolean values in case of AnyBoolean
            var leftAnyBoolean = leftAnyValues.Any(a => a is AnyBooleanValue);
            var rightAnyBoolean = rightAnyValues.Any(a => a is AnyBooleanValue);
            if (leftAnyBoolean && rightAnyBoolean)
            {
                result.Add(valueFactory.Snapshot.AnyBooleanValue);
            }
            else if (leftAnyBoolean)
            {
                result.AddRange(rightValues.Where(a => a is ScalarValue<bool>));
            }
            else if (rightAnyBoolean)
            {
                result.AddRange(leftValues.Where(a => a is ScalarValue<bool>));
            }

            // Intersect string values in case of AnyString
            var leftAnyString = leftAnyValues.Any(a => a is AnyStringValue);
            var rightAnyString = rightAnyValues.Any(a => a is AnyStringValue);
            if (leftAnyString && rightAnyString)
            {
                result.Add(valueFactory.Snapshot.AnyStringValue);
            }
            else if (leftAnyString)
            {
                result.AddRange(rightValues.Where(a => a is StringValue));
            }
            else if (rightAnyString)
            {
                result.AddRange(leftValues.Where(a => a is StringValue));
            }
            

            // Intersect numeric values in case of AnyBoolean and numeric interval values
            var leftAnyNumeric = leftAnyValues.Where(a => a is AnyNumericValue);
            var rightAnyNumeric = rightAnyValues.Where(a => a is AnyNumericValue);
            if (leftAnyNumeric.Count() > 0 && rightAnyNumeric.Count() > 0)
            {
                result.AddRange(leftAnyNumeric);
                result.AddRange(rightAnyNumeric);
            }
            else if (leftAnyNumeric.Count() > 0)
            {
                result.AddRange(rightValues.Where(a => a is NumericValue));
            }
            else if (rightAnyNumeric.Count() > 0)
            {
                result.AddRange(leftValues.Where(a => a is NumericValue));
            }
            else
            {
                // intersect interval values
                var leftIntervalValues = leftValues.Where(a => a is IntervalValue).Select(a => (IntervalValue)a);
                var rightIntervalValues = rightValues.Where(a => a is IntervalValue).Select(a => (IntervalValue)a);
                foreach (var leftInterval in leftIntervalValues)
                {
                    foreach (var rightInterval in rightIntervalValues)
                    {
                        // intersect two intervals
                        var rightIntervalList = new List<IntervalValue>(1);
                        rightIntervalList.Add(rightInterval);
                        var intersected = IntersectIntervals(leftInterval, rightIntervalList);
                        // if the intersection is not empty interval, add to the result
                        if (!(intersected is UndefinedValue))
                            result.Add(intersected);
                    }
                }
            }

            // Intersect non-interval values
            result.AddRange(leftValues.Intersect(rightValues));

            

            return result;
        }

        void IntersectValues(List<Value> values, Value newValue)
        {
            if (newValue is ConcreteValue)
            {
                //There is nothin more precise, so we can delete anything.
                //If there is a precise value already present, the newValue should be one of them.
                values.Clear();
                values.Add(newValue);
                return;
            }



            if (newValue is IntervalValue)
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
            if (value is UndefinedValue) return true;

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
                // Empty interval
                return valueFactory.UndefinedValue;
            }
        }

        #endregion
    }
}