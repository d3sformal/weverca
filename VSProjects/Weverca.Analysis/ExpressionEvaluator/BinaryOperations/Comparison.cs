/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

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

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class contains methods performing value comparison.
    /// </summary>
    /// <remarks>
    /// In PHP language, method of comparison varies depending on type of operands in this order:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Comparison of strings is lexicographic and abstract strings cannot be compared.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If one operand is boolean value, the other one is converted into boolean too. Abstract values are
    /// converted into an abstract boolean value expect of intervals that can take a concrete boolean value
    /// in some cases. Comparison can result to concrete boolean value even if one operand is abstract.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The comparison of numbers is simple. If we compare number intervals, the result is <c>true</c>
    /// respectively <c>false</c> if all combinations of interval value comparing is <c>true</c>
    /// respectively <c>false</c>. Comparing with an abstract number cannot be simply evaluated.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Other comparisons are very specific and their resolving is not such simple. Some operand types are
    /// always greater than other not depending on their values (e.g. object is always greater than array).
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class Comparison
    {
        /// <summary>
        /// Compare values of the same type with the specific operation.
        /// </summary>
        /// <remarks>
        /// Note that the method is generic and can be applied for all types that defines comparing.
        /// There is one exception. The string values must be compared by the specialized method,
        /// because default comparing of strings differs from the way the PHP compares them.
        /// </remarks>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left operand to compare.</param>
        /// <param name="rightOperand">Right operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static BooleanValue Compare<T>(ISnapshotReadWrite snapshot, Operations operation,
            T leftOperand, T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return snapshot.CreateBool(leftOperand.Equals(rightOperand));
                case Operations.NotEqual:
                    return snapshot.CreateBool(!leftOperand.Equals(rightOperand));
                case Operations.LessThan:
                    return snapshot.CreateBool(leftOperand.CompareTo(rightOperand) < 0);
                case Operations.LessThanOrEqual:
                    return snapshot.CreateBool(leftOperand.CompareTo(rightOperand) <= 0);
                case Operations.GreaterThan:
                    return snapshot.CreateBool(leftOperand.CompareTo(rightOperand) > 0);
                case Operations.GreaterThanOrEqual:
                    return snapshot.CreateBool(leftOperand.CompareTo(rightOperand) >= 0);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Compare string representations with the specific operation.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left string operand to compare.</param>
        /// <param name="rightOperand">Right string operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static BooleanValue Compare(ISnapshotReadWrite snapshot, Operations operation,
            string leftOperand, string rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return snapshot.CreateBool(string.Equals(leftOperand, rightOperand,
                        StringComparison.Ordinal));
                case Operations.NotEqual:
                    return snapshot.CreateBool(!string.Equals(leftOperand, rightOperand,
                        StringComparison.Ordinal));
                case Operations.LessThan:
                    return snapshot.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) < 0);
                case Operations.LessThanOrEqual:
                    return snapshot.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) <= 0);
                case Operations.GreaterThan:
                    return snapshot.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) > 0);
                case Operations.GreaterThanOrEqual:
                    return snapshot.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) >= 0);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Compare left boolean operand to right number interval operand with the specified operation.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left boolean operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value IntervalCompare<T>(ISnapshotReadWrite snapshot, Operations operation,
            bool leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(rightOperand, out convertedValue))
            {
                return Compare(snapshot, operation, leftOperand, convertedValue);
            }
            else
            {
                return RightAbstractBooleanCompare(snapshot, operation, leftOperand);
            }
        }

        /// <summary>
        /// Compare left number operand to right number interval operand of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value IntervalCompare<T>(ISnapshotReadWrite snapshot, Operations operation,
            T leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return Equal(snapshot, leftOperand, rightOperand);
                case Operations.NotEqual:
                    return NotEqual(snapshot, leftOperand, rightOperand);
                case Operations.LessThan:
                    return LessThan(snapshot, leftOperand, rightOperand);
                case Operations.LessThanOrEqual:
                    return LessThanOrEqual(snapshot, leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return GreaterThan(snapshot, leftOperand, rightOperand);
                case Operations.GreaterThanOrEqual:
                    return GreaterThanOrEqual(snapshot, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Compare left number interval operand to right boolean operand with the specified operation.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right boolean operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value IntervalCompare<T>(ISnapshotReadWrite snapshot, Operations operation,
            IntervalValue<T> leftOperand, bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(leftOperand, out convertedValue))
            {
                return Compare(snapshot, operation, convertedValue, rightOperand);
            }
            else
            {
                return LeftAbstractBooleanCompare(snapshot, operation, rightOperand);
            }
        }

        /// <summary>
        /// Compare left number interval operand to right number operand of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value IntervalCompare<T>(ISnapshotReadWrite snapshot, Operations operation,
            IntervalValue<T> leftOperand, T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return Equal(snapshot, leftOperand, rightOperand);
                case Operations.NotEqual:
                    return NotEqual(snapshot, leftOperand, rightOperand);
                case Operations.LessThan:
                    return LessThan(snapshot, leftOperand, rightOperand);
                case Operations.LessThanOrEqual:
                    return LessThanOrEqual(snapshot, leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return GreaterThan(snapshot, leftOperand, rightOperand);
                case Operations.GreaterThanOrEqual:
                    return GreaterThanOrEqual(snapshot, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Compare number interval operands of the same type with the specified operation.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value IntervalCompare<T>(ISnapshotReadWrite snapshot, Operations operation,
            IntervalValue<T> leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return Equal(snapshot, leftOperand, rightOperand);
                case Operations.NotEqual:
                    return NotEqual(snapshot, leftOperand, rightOperand);
                case Operations.LessThan:
                    return LessThan(snapshot, leftOperand, rightOperand);
                case Operations.LessThanOrEqual:
                    return LessThanOrEqual(snapshot, leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return GreaterThan(snapshot, leftOperand, rightOperand);
                case Operations.GreaterThanOrEqual:
                    return GreaterThanOrEqual(snapshot, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Return result of comparison operation where the left operand is less than the right operand.
        /// </summary>
        /// <remarks>
        /// In PHP language, object is always greater than string (if it has not "__toString" magic method
        /// implemented), array, resource and null value. Array is always greater than string and resource.
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static BooleanValue RightAlwaysGreater(ISnapshotReadWrite snapshot, Operations operation)
        {
            switch (operation)
            {
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                    return snapshot.CreateBool(true);
                case Operations.Equal:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                    return snapshot.CreateBool(false);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Return result of comparison operation where the left operand is greater than the right operand.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        /// <seealso cref="RightAlwaysGreater"/>
        public static BooleanValue LeftAlwaysGreater(ISnapshotReadWrite snapshot, Operations operation)
        {
            switch (operation)
            {
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                    return snapshot.CreateBool(true);
                case Operations.Equal:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                    return snapshot.CreateBool(false);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform comparison of boolean values where only the left boolean operand is known.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left boolean operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanCompare(ISnapshotReadWrite snapshot,
            Operations operation, bool leftOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return snapshot.AnyBooleanValue;
                case Operations.NotEqual:
                    return snapshot.AnyBooleanValue;
                case Operations.LessThan:
                    if (leftOperand)
                    {
                        return snapshot.CreateBool(false);
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                case Operations.LessThanOrEqual:
                    if (leftOperand)
                    {
                        return snapshot.AnyBooleanValue;
                    }
                    else
                    {
                        return snapshot.CreateBool(true);
                    }
                case Operations.GreaterThan:
                    if (leftOperand)
                    {
                        return snapshot.AnyBooleanValue;
                    }
                    else
                    {
                        return snapshot.CreateBool(false);
                    }
                case Operations.GreaterThanOrEqual:
                    if (leftOperand)
                    {
                        return snapshot.CreateBool(true);
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform comparison of boolean values where only the left number interval operand is known.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanCompare<T>(ISnapshotReadWrite snapshot,
            Operations operation, IntervalValue<T> leftOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(leftOperand, out convertedValue))
            {
                return RightAbstractBooleanCompare(snapshot, operation, convertedValue);
            }
            else
            {
                return AbstractCompare(snapshot, operation);
            }
        }

        /// <summary>
        /// Perform comparison of boolean values where only the right boolean operand is known.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="rightOperand">Right boolean operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanCompare(ISnapshotReadWrite snapshot,
            Operations operation, bool rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return snapshot.AnyBooleanValue;
                case Operations.NotEqual:
                    return snapshot.AnyBooleanValue;
                case Operations.LessThan:
                    if (rightOperand)
                    {
                        return snapshot.AnyBooleanValue;
                    }
                    else
                    {
                        return snapshot.CreateBool(false);
                    }
                case Operations.LessThanOrEqual:
                    if (rightOperand)
                    {
                        return snapshot.CreateBool(true);
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                case Operations.GreaterThan:
                    if (rightOperand)
                    {
                        return snapshot.CreateBool(false);
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                case Operations.GreaterThanOrEqual:
                    if (rightOperand)
                    {
                        return snapshot.AnyBooleanValue;
                    }
                    else
                    {
                        return snapshot.CreateBool(true);
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform comparison of boolean values where only the right number interval operand is known.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>If operation is comparison, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanCompare<T>(ISnapshotReadWrite snapshot,
            Operations operation, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(rightOperand, out convertedValue))
            {
                return LeftAbstractBooleanCompare(snapshot, operation, convertedValue);
            }
            else
            {
                return AbstractCompare(snapshot, operation);
            }
        }

        /// <summary>
        /// Return an abstract boolean result of comparison when operands are unknown.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only comparison gives a result.</param>
        /// <returns>If operation is comparison, it returns any boolean, otherwise <c>null</c>.</returns>
        public static AnyBooleanValue AbstractCompare(ISnapshotReadWrite snapshot, Operations operation)
        {
            return IsOperationComparison(operation) ? snapshot.AnyBooleanValue : null;
        }

        /// <summary>
        /// Indicate whether the given operation is comparison.
        /// </summary>
        /// <param name="operation">Operation to be checked.</param>
        /// <returns><c>true</c> whether operation is comparison, otherwise <c>false</c></returns>
        public static bool IsOperationComparison(Operations operation)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                    return true;
                default:
                    return false;
            }
        }

        #region Equal

        /// <summary>
        /// Compare concrete number to number interval of the same type for equality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value Equal<T>(ISnapshotReadWrite snapshot, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.CompareTo(rightOperand.Start) >= 0)
                && (leftOperand.CompareTo(rightOperand.End) <= 0))
            {
                if (leftOperand.Equals(rightOperand.Start) && leftOperand.Equals(rightOperand.End))
                {
                    return snapshot.CreateBool(true);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
            else
            {
                return snapshot.CreateBool(false);
            }
        }

        /// <summary>
        /// Compare number interval to concrete number of the same type for equality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value Equal<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Equal(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare two number intervals of the same type for equality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value Equal<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.End.CompareTo(rightOperand.Start) >= 0)
                && (leftOperand.Start.CompareTo(rightOperand.End) <= 0))
            {
                if (leftOperand.Start.Equals(leftOperand.End)
                    && leftOperand.Start.Equals(rightOperand.Start)
                    && leftOperand.End.Equals(rightOperand.End))
                {
                    return snapshot.CreateBool(true);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
            else
            {
                return snapshot.CreateBool(false);
            }
        }

        #endregion Equal

        #region NotEqual

        /// <summary>
        /// Compare concrete number to number interval of the same type for inequality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value NotEqual<T>(ISnapshotReadWrite snapshot, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.CompareTo(rightOperand.Start) < 0)
                || (leftOperand.CompareTo(rightOperand.End) > 0))
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                if (leftOperand.Equals(rightOperand.Start) && leftOperand.Equals(rightOperand.End))
                {
                    return snapshot.CreateBool(false);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
        }

        /// <summary>
        /// Compare number interval to concrete number of the same type for inequality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value NotEqual<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return NotEqual(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare two number intervals of the same type for inequality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value NotEqual<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.End.CompareTo(rightOperand.Start) < 0)
                || (leftOperand.Start.CompareTo(rightOperand.End) > 0))
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                if (leftOperand.Start.Equals(leftOperand.End)
                    && leftOperand.Start.Equals(rightOperand.Start)
                    && leftOperand.End.Equals(rightOperand.End))
                {
                    return snapshot.CreateBool(false);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
        }

        #endregion NotEqual

        #region LessThan

        /// <summary>
        /// Compare whether concrete number is less than number interval of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value LessThan<T>(ISnapshotReadWrite snapshot, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) < 0)
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                if (leftOperand.CompareTo(rightOperand.End) < 0)
                {
                    return snapshot.AnyBooleanValue;
                }
                else
                {
                    return snapshot.CreateBool(false);
                }
            }
        }

        /// <summary>
        /// Compare whether number interval is less than concrete number of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value LessThan<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return GreaterThan(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare whether left interval operand is less than right interval operand of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value LessThan<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.End.CompareTo(rightOperand.Start) < 0)
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                if (leftOperand.Start.CompareTo(rightOperand.End) < 0)
                {
                    return snapshot.AnyBooleanValue;
                }
                else
                {
                    return snapshot.CreateBool(false);
                }
            }
        }

        #endregion LessThan

        #region LessThanOrEqual

        /// <summary>
        /// Compare whether concrete number is less than or equal to number interval of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value LessThanOrEqual<T>(ISnapshotReadWrite snapshot, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) <= 0)
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                if (leftOperand.CompareTo(rightOperand.End) <= 0)
                {
                    return snapshot.AnyBooleanValue;
                }
                else
                {
                    return snapshot.CreateBool(false);
                }
            }
        }

        /// <summary>
        /// Compare whether number interval is less than or equal to concrete number of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value LessThanOrEqual<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return GreaterThanOrEqual(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare whether left interval operand is less than or equal to right interval operand.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value LessThanOrEqual<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.End.CompareTo(rightOperand.Start) <= 0)
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                if (leftOperand.Start.CompareTo(rightOperand.End) <= 0)
                {
                    return snapshot.AnyBooleanValue;
                }
                else
                {
                    return snapshot.CreateBool(false);
                }
            }
        }

        #endregion LessThanOrEqual

        #region GreaterThan

        /// <summary>
        /// Compare whether concrete number is greater than number interval of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value GreaterThan<T>(ISnapshotReadWrite snapshot, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) > 0)
            {
                if (leftOperand.CompareTo(rightOperand.End) > 0)
                {
                    return snapshot.CreateBool(true);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
            else
            {
                return snapshot.CreateBool(false);
            }
        }

        /// <summary>
        /// Compare whether number interval is greater than concrete number of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value GreaterThan<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThan(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare whether left interval operand is greater than right interval operand of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value GreaterThan<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
             IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThan(snapshot, rightOperand, leftOperand);
        }

        #endregion GreaterThan

        #region GreaterThanOrEqual

        /// <summary>
        /// Compare whether concrete number is greater than or equal to number interval of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left concrete number operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value GreaterThanOrEqual<T>(ISnapshotReadWrite snapshot, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) >= 0)
            {
                if (leftOperand.CompareTo(rightOperand.End) >= 0)
                {
                    return snapshot.CreateBool(true);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
            else
            {
                return snapshot.CreateBool(false);
            }
        }

        /// <summary>
        /// Compare whether number interval is greater than or equal to concrete number of the same type.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right concrete number operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value GreaterThanOrEqual<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThanOrEqual(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare whether left interval operand is greater than or equal to right interval operand.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left number interval operand to compare.</param>
        /// <param name="rightOperand">Right number interval operand to compare.</param>
        /// <returns>Boolean value obtained by comparison of all value combinations.</returns>
        public static Value GreaterThanOrEqual<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThanOrEqual(snapshot, rightOperand, leftOperand);
        }

        #endregion GreaterThanOrEqual
    }
}