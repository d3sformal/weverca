using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
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
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="leftOperand">Left operand to compare</param>
        /// <param name="rightOperand">Right operand to compare</param>
        /// <returns>Boolean result of comparison if this is the operation, otherwise <c>null</c></returns>
        public static BooleanValue Compare<T>(FlowOutputSet outset, Operations operation,
            T leftOperand, T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return outset.CreateBool(leftOperand.Equals(rightOperand));
                case Operations.NotEqual:
                    return outset.CreateBool(!leftOperand.Equals(rightOperand));
                case Operations.LessThan:
                    return outset.CreateBool(leftOperand.CompareTo(rightOperand) < 0);
                case Operations.LessThanOrEqual:
                    return outset.CreateBool(leftOperand.CompareTo(rightOperand) <= 0);
                case Operations.GreaterThan:
                    return outset.CreateBool(leftOperand.CompareTo(rightOperand) > 0);
                case Operations.GreaterThanOrEqual:
                    return outset.CreateBool(leftOperand.CompareTo(rightOperand) >= 0);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Compare string representations with the specific operation.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="leftOperand">Left string operand to compare</param>
        /// <param name="rightOperand">Right string operand to compare</param>
        /// <returns>Boolean result of comparison if this is the operation, otherwise <c>null</c></returns>
        public static BooleanValue Compare(FlowOutputSet outset, Operations operation,
            string leftOperand, string rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return outset.CreateBool(string.Equals(leftOperand, rightOperand,
                        StringComparison.Ordinal));
                case Operations.NotEqual:
                    return outset.CreateBool(!string.Equals(leftOperand, rightOperand,
                        StringComparison.Ordinal));
                case Operations.LessThan:
                    return outset.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) < 0);
                case Operations.LessThanOrEqual:
                    return outset.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) <= 0);
                case Operations.GreaterThan:
                    return outset.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) > 0);
                case Operations.GreaterThanOrEqual:
                    return outset.CreateBool(string.Compare(leftOperand, rightOperand,
                        StringComparison.Ordinal) >= 0);
                default:
                    return null;
            }
        }

        public static Value IntervalCompare<T>(FlowOutputSet outset, Operations operation,
            bool leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(rightOperand, out convertedValue))
            {
                return Compare(outset, operation, leftOperand, convertedValue);
            }
            else
            {
                return RightAbstractBooleanCompare(outset, operation, leftOperand);
            }
        }

        /// <summary>
        /// Compare concrete integer to integer interval with the specified operation.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="leftOperand">Left concrete integer operand to compare</param>
        /// <param name="rightOperand">Right integer interval operand to compare</param>
        /// <returns>Boolean result of comparison if this is the operation, otherwise <c>null</c></returns>
        public static Value IntervalCompare<T>(FlowOutputSet outset, Operations operation,
            T leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return Equal(outset, leftOperand, rightOperand);
                case Operations.NotEqual:
                    return NotEqual(outset, leftOperand, rightOperand);
                case Operations.LessThan:
                    return LessThan(outset, leftOperand, rightOperand);
                case Operations.LessThanOrEqual:
                    return LessThanOrEqual(outset, leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return GreaterThan(outset, leftOperand, rightOperand);
                case Operations.GreaterThanOrEqual:
                    return GreaterThanOrEqual(outset, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        public static Value IntervalCompare<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> leftOperand, bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(leftOperand, out convertedValue))
            {
                return Compare(outset, operation, convertedValue, rightOperand);
            }
            else
            {
                return LeftAbstractBooleanCompare(outset, operation, rightOperand);
            }
        }

        /// <summary>
        /// Compare integer interval to concrete integer with the specified operation.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="leftOperand">Left integer interval operand to compare</param>
        /// <param name="rightOperand">Right concrete integer operand to compare</param>
        /// <returns>Boolean result of comparison if this is the operation, otherwise <c>null</c></returns>
        public static Value IntervalCompare<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> leftOperand, T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return Equal(outset, leftOperand, rightOperand);
                case Operations.NotEqual:
                    return NotEqual(outset, leftOperand, rightOperand);
                case Operations.LessThan:
                    return LessThan(outset, leftOperand, rightOperand);
                case Operations.LessThanOrEqual:
                    return LessThanOrEqual(outset, leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return GreaterThan(outset, leftOperand, rightOperand);
                case Operations.GreaterThanOrEqual:
                    return GreaterThanOrEqual(outset, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Compare integer intervals with the specified operation.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="operation">Binary operation</param>
        /// <param name="leftOperand">Left integer interval operand to compare</param>
        /// <param name="rightOperand">Right integer interval operand to compare</param>
        /// <returns>Boolean result of comparison if this is the operation, otherwise <c>null</c></returns>
        public static Value IntervalCompare<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.Equal:
                    return Equal(outset, leftOperand, rightOperand);
                case Operations.NotEqual:
                    return NotEqual(outset, leftOperand, rightOperand);
                case Operations.LessThan:
                    return LessThan(outset, leftOperand, rightOperand);
                case Operations.LessThanOrEqual:
                    return LessThanOrEqual(outset, leftOperand, rightOperand);
                case Operations.GreaterThan:
                    return GreaterThan(outset, leftOperand, rightOperand);
                case Operations.GreaterThanOrEqual:
                    return GreaterThanOrEqual(outset, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        public static BooleanValue RightAlwaysGreater(FlowOutputSet outset, Operations operation)
        {
            switch (operation)
            {
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                    return outset.CreateBool(true);
                case Operations.Equal:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                    return outset.CreateBool(false);
                default:
                    return null;
            }
        }

        public static BooleanValue LeftAlwaysGreater(FlowOutputSet outset, Operations operation)
        {
            switch (operation)
            {
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                    return outset.CreateBool(true);
                case Operations.Equal:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                    return outset.CreateBool(false);
                default:
                    return null;
            }
        }

        public static Value RightAbstractBooleanCompare(FlowOutputSet outset,
            Operations operation, bool leftOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return outset.AnyBooleanValue;
                case Operations.NotEqual:
                    return outset.AnyBooleanValue;
                case Operations.LessThan:
                    if (leftOperand)
                    {
                        return outset.CreateBool(false);
                    }
                    else
                    {
                        return outset.AnyBooleanValue;
                    }
                case Operations.LessThanOrEqual:
                    if (leftOperand)
                    {
                        return outset.AnyBooleanValue;
                    }
                    else
                    {
                        return outset.CreateBool(true);
                    }
                case Operations.GreaterThan:
                    if (leftOperand)
                    {
                        return outset.AnyBooleanValue;
                    }
                    else
                    {
                        return outset.CreateBool(false);
                    }
                case Operations.GreaterThanOrEqual:
                    if (leftOperand)
                    {
                        return outset.CreateBool(true);
                    }
                    else
                    {
                        return outset.AnyBooleanValue;
                    }
                default:
                    return null;
            }
        }

        public static Value RightAbstractBooleanCompare<T>(FlowOutputSet outset,
            Operations operation, IntervalValue<T> leftOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(leftOperand, out convertedValue))
            {
                return RightAbstractBooleanCompare(outset, operation, convertedValue);
            }
            else
            {
                return AbstractCompare(outset, operation);
            }
        }

        public static Value LeftAbstractBooleanCompare(FlowOutputSet outset,
            Operations operation, bool rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                    return outset.AnyBooleanValue;
                case Operations.NotEqual:
                    return outset.AnyBooleanValue;
                case Operations.LessThan:
                    if (rightOperand)
                    {
                        return outset.AnyBooleanValue;
                    }
                    else
                    {
                        return outset.CreateBool(false);
                    }
                case Operations.LessThanOrEqual:
                    if (rightOperand)
                    {
                        return outset.CreateBool(true);
                    }
                    else
                    {
                        return outset.AnyBooleanValue;
                    }
                case Operations.GreaterThan:
                    if (rightOperand)
                    {
                        return outset.CreateBool(false);
                    }
                    else
                    {
                        return outset.AnyBooleanValue;
                    }
                case Operations.GreaterThanOrEqual:
                    if (rightOperand)
                    {
                        return outset.AnyBooleanValue;
                    }
                    else
                    {
                        return outset.CreateBool(true);
                    }
                default:
                    return null;
            }
        }

        public static Value LeftAbstractBooleanCompare<T>(FlowOutputSet outset,
            Operations operation, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;
            if (TypeConversion.TryConvertToBoolean(rightOperand, out convertedValue))
            {
                return LeftAbstractBooleanCompare(outset, operation, convertedValue);
            }
            else
            {
                return AbstractCompare(outset, operation);
            }
        }

        public static AnyBooleanValue AbstractCompare(FlowOutputSet outset, Operations operation)
        {
            return IsOperationComparison(operation) ? outset.AnyBooleanValue : null;
        }

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
        /// Compare concrete number of any type to number interval of other type for equality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="leftOperand">Left concrete number operand to compare</param>
        /// <param name="rightOperand">Right number interval operand to compare</param>
        /// <returns>Boolean value obtained by comparison of all value combinations</returns>
        public static Value Equal<T>(FlowOutputSet outset, T leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.CompareTo(rightOperand.Start) >= 0)
                && (leftOperand.CompareTo(rightOperand.End) <= 0))
            {
                if (leftOperand.Equals(rightOperand.Start) && leftOperand.Equals(rightOperand.End))
                {
                    return outset.CreateBool(true);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
            else
            {
                return outset.CreateBool(false);
            }
        }

        /// <summary>
        /// Compare number interval of any type to concrete number of other type for equality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="leftOperand">Left number interval operand to compare</param>
        /// <param name="rightOperand">Right concrete number operand to compare</param>
        /// <returns>Boolean value obtained by comparison of all value combinations</returns>
        public static Value Equal<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Equal(outset, rightOperand, leftOperand);
        }

        /// <summary>
        /// Compare two number intervals of possible different types for equality.
        /// </summary>
        /// <typeparam name="T">Comparable type of the operands</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="leftOperand">Left number interval operand to compare</param>
        /// <param name="rightOperand">Right umber interval operand to compare</param>
        /// <returns>Boolean value obtained by comparison of all value combinations</returns>
        public static Value Equal<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
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
                    return outset.CreateBool(true);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
            else
            {
                return outset.CreateBool(false);
            }
        }

        #endregion Equal

        #region NotEqual

        public static Value NotEqual<T>(FlowOutputSet outset, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.CompareTo(rightOperand.Start) < 0)
                || (leftOperand.CompareTo(rightOperand.End) > 0))
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.Equals(rightOperand.Start) && leftOperand.Equals(rightOperand.End))
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        public static Value NotEqual<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return NotEqual(outset, rightOperand, leftOperand);
        }

        public static Value NotEqual<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((leftOperand.End.CompareTo(rightOperand.Start) < 0)
                || (leftOperand.Start.CompareTo(rightOperand.End) > 0))
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.Start.Equals(leftOperand.End)
                    && leftOperand.Start.Equals(rightOperand.Start)
                    && leftOperand.End.Equals(rightOperand.End))
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        #endregion NotEqual

        #region LessThan

        public static Value LessThan<T>(FlowOutputSet outset, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) < 0)
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.CompareTo(rightOperand.End) >= 0)
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        public static Value LessThan<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return GreaterThan(outset, rightOperand, leftOperand);
        }

        public static Value LessThan<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.End.CompareTo(rightOperand.Start) < 0)
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.Start.CompareTo(rightOperand.End) >= 0)
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        #endregion LessThan

        #region LessThanOrEqual

        public static Value LessThanOrEqual<T>(FlowOutputSet outset, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) <= 0)
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.CompareTo(rightOperand.End) > 0)
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        public static Value LessThanOrEqual<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return GreaterThanOrEqual(outset, rightOperand, leftOperand);
        }

        public static Value LessThanOrEqual<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.End.CompareTo(rightOperand.Start) <= 0)
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.Start.CompareTo(rightOperand.End) > 0)
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        #endregion LessThanOrEqual

        #region GreaterThan

        public static Value GreaterThan<T>(FlowOutputSet outset, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) > 0)
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.CompareTo(rightOperand.End) <= 0)
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        public static Value GreaterThan<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThan(outset, rightOperand, leftOperand);
        }

        public static Value GreaterThan<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
             IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThan(outset, rightOperand, leftOperand);
        }

        #endregion GreaterThan

        #region GreaterThanOrEqual

        public static Value GreaterThanOrEqual<T>(FlowOutputSet outset, T leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if (leftOperand.CompareTo(rightOperand.Start) >= 0)
            {
                return outset.CreateBool(true);
            }
            else
            {
                if (leftOperand.CompareTo(rightOperand.End) < 0)
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
        }

        public static Value GreaterThanOrEqual<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            T rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThanOrEqual(outset, rightOperand, leftOperand);
        }

        public static Value GreaterThanOrEqual<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return LessThanOrEqual(outset, rightOperand, leftOperand);
        }

        #endregion GreaterThanOrEqual
    }
}
