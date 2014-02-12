using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class contains methods performing logical operations.
    /// </summary>
    /// <remarks>
    /// When PHP performs a logical operation, it always converts both operands into booleans. Conversion
    /// into boolean is defined for every type in <see cref="TypeConversion" />. Abstract values are
    /// converted into an abstract boolean value expect of intervals that can take a concrete boolean value
    /// in some cases. The AND respectively OR operation has the advantage that if one operand is known,
    /// <c>false</c> respectively <c>true</c>, the result is <c>false</c> respectively <c>true</c>.
    /// </remarks>
    /// <seealso cref="TypeConversion.TryConvertToBoolean{T}(IntervalValue{T}, out bool)" />
    public static class LogicalOperation
    {
        /// <summary>
        /// Perform logical operation for given boolean operands.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left boolean operand of logical operation.</param>
        /// <param name="rightOperand">Right integer operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static BooleanValue Logical(FlowOutputSet outset, Operations operation,
            bool leftOperand, bool rightOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    return outset.CreateBool(leftOperand && rightOperand);
                case Operations.Or:
                    return outset.CreateBool(leftOperand || rightOperand);
                case Operations.Xor:
                    return outset.CreateBool(leftOperand != rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical operation for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left boolean operand of logical operation.</param>
        /// <param name="rightOperand">Right interval operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value Logical<T>(FlowOutputSet outset, Operations operation,
            bool leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.And:
                    return And(outset, leftOperand, rightOperand);
                case Operations.Or:
                    return Or(outset, leftOperand, rightOperand);
                case Operations.Xor:
                    return Xor(outset, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical operation for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left interval operand of logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value Logical<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> leftOperand, bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Logical(outset, operation, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical operation for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left interval operand of logical operation.</param>
        /// <param name="rightOperand">Right interval operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value Logical<TLeft, TRight>(FlowOutputSet outset, Operations operation,
            IntervalValue<TLeft> leftOperand, IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            switch (operation)
            {
                case Operations.And:
                    return And(outset, leftOperand, rightOperand);
                case Operations.Or:
                    return Or(outset, leftOperand, rightOperand);
                case Operations.Xor:
                    return Xor(outset, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical operation for one given boolean operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="concreteOperand">One concrete boolean operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value AbstractLogical(FlowOutputSet outset, Operations operation,
            bool concreteOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    return AbstractAnd(outset, concreteOperand);
                case Operations.Or:
                    return AbstractOr(outset, concreteOperand);
                case Operations.Xor:
                    return AbstractXor(outset);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical operation for one given interval operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="intervalOperand">One specified interval operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value AbstractLogical<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> intervalOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.And:
                    return AbstractAnd(outset, intervalOperand);
                case Operations.Or:
                    return AbstractOr(outset, intervalOperand);
                case Operations.Xor:
                    return AbstractXor(outset);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Return an abstract boolean result of logical operation when operands are unknown.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <returns>If operation is logical, it returns abstract boolean, otherwise <c>null</c>.</returns>
        public static AnyBooleanValue AbstractLogical(FlowOutputSet outset, Operations operation)
        {
            return IsLogical(operation) ? outset.AnyBooleanValue : null;
        }

        /// <summary>
        /// Indicate whether the given operation is logical.
        /// </summary>
        /// <param name="operation">Operation to be checked.</param>
        /// <returns><c>true</c> whether operation is logical, otherwise <c>false</c></returns>
        public static bool IsLogical(Operations operation)
        {
            switch (operation)
            {
                case Operations.And:
                case Operations.Or:
                case Operations.Xor:
                    return true;
                default:
                    return false;
            }
        }

        #region And

        /// <summary>
        /// Perform logical AND for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left boolean operand of AND logical operation.</param>
        /// <param name="rightOperand">Right interval operand of AND logical operation.</param>
        /// <returns><c>true</c> whether both operands are <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value And<T>(FlowOutputSet outset, bool leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
            {
                return outset.CreateBool(leftOperand && convertedValue);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical AND for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left interval operand of AND logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of AND logical operation.</param>
        /// <returns><c>true</c> whether both operands are <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value And<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return And(outset, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical AND for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left interval operand of AND logical operation.</param>
        /// <param name="rightOperand">Right interval operand of AND logical operation.</param>
        /// <returns><c>true</c> whether both operands are <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value And<TLeft, TRight>(FlowOutputSet outset, IntervalValue<TLeft> leftOperand,
            IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<TLeft>(leftOperand, out convertedValue))
            {
                return And(outset, convertedValue, rightOperand);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical AND for one given boolean operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="concreteOperand">One concrete boolean operand of AND logical operation.</param>
        /// <returns><c>false</c> whether the operand is <c>false</c>, otherwise abstract boolean.</returns>
        public static Value AbstractAnd(FlowOutputSet outset, bool concreteOperand)
        {
            if (concreteOperand)
            {
                return outset.AnyBooleanValue;
            }
            else
            {
                return outset.CreateBool(false);
            }
        }

        /// <summary>
        /// Perform logical AND for one given interval operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="intervalOperand">One specified interval operand of AND logical operation.</param>
        /// <returns><c>false</c> whether the operand is <c>false</c>, otherwise abstract boolean.</returns>
        public static Value AbstractAnd<T>(FlowOutputSet outset, IntervalValue<T> intervalOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(intervalOperand, out convertedValue))
            {
                return AbstractAnd(outset, convertedValue);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        #endregion And

        #region Or

        /// <summary>
        /// Perform logical OR for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left boolean operand of OR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of OR logical operation.</param>
        /// <returns><c>true</c> whether either operand is <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value Or<T>(FlowOutputSet outset, bool leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
            {
                return outset.CreateBool(leftOperand || convertedValue);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical OR for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left interval operand of OR logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of OR logical operation.</param>
        /// <returns><c>true</c> whether either operand is <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value Or<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Or(outset, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical OR for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left interval operand of OR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of OR logical operation.</param>
        /// <returns><c>true</c> whether either operand is <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value Or<TLeft, TRight>(FlowOutputSet outset, IntervalValue<TLeft> leftOperand,
            IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<TLeft>(leftOperand, out convertedValue))
            {
                return Or(outset, convertedValue, rightOperand);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical OR for one given boolean operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="concreteOperand">One concrete boolean operand of OR logical operation.</param>
        /// <returns><c>true</c> whether the operand is <c>true</c>, otherwise abstract boolean.</returns>
        public static Value AbstractOr(FlowOutputSet outset, bool concreteOperand)
        {
            if (concreteOperand)
            {
                return outset.CreateBool(true);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical OR for one given interval operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="intervalOperand">One specified interval operand of OR logical operation.</param>
        /// <returns><c>true</c> whether the operand is <c>true</c>, otherwise abstract boolean.</returns>
        public static Value AbstractOr<T>(FlowOutputSet outset, IntervalValue<T> intervalOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(intervalOperand, out convertedValue))
            {
                return AbstractOr(outset, convertedValue);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        #endregion Or

        #region Xor

        /// <summary>
        /// Perform logical XOR for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left boolean operand of XOR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of XOR logical operation.</param>
        /// <returns><c>true</c> whether operands are not equal, otherwise <c>false</c>.</returns>
        public static Value Xor<T>(FlowOutputSet outset, bool leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
            {
                return outset.CreateBool(leftOperand != convertedValue);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical XOR for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left interval operand of XOR logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of XOR logical operation.</param>
        /// <returns><c>true</c> whether operands are not equal, otherwise <c>false</c>.</returns>
        public static Value Xor<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Xor(outset, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical XOR for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="outset">Output set of a program point.</param>
        /// <param name="leftOperand">Left interval operand of XOR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of XOR logical operation.</param>
        /// <returns><c>true</c> whether operands are not equal, otherwise <c>false</c>.</returns>
        public static Value Xor<TLeft, TRight>(FlowOutputSet outset, IntervalValue<TLeft> leftOperand,
            IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<TLeft>(leftOperand, out convertedValue))
            {
                return Xor(outset, convertedValue, rightOperand);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Return an abstract boolean result of XOR logical operation even if at least one operand is known.
        /// </summary>
        /// <param name="outset">Output set of a program point.</param>
        /// <returns>An abstract boolean.</returns>
        public static AnyBooleanValue AbstractXor(FlowOutputSet outset)
        {
            return outset.AnyBooleanValue;
        }

        #endregion Xor
    }
}
