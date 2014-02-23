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
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left boolean operand of logical operation.</param>
        /// <param name="rightOperand">Right integer operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static BooleanValue Logical(ISnapshotReadWrite snapshot, Operations operation,
            bool leftOperand, bool rightOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    return snapshot.CreateBool(leftOperand && rightOperand);
                case Operations.Or:
                    return snapshot.CreateBool(leftOperand || rightOperand);
                case Operations.Xor:
                    return snapshot.CreateBool(leftOperand != rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical operation for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left boolean operand of logical operation.</param>
        /// <param name="rightOperand">Right interval operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value Logical<T>(ISnapshotReadWrite snapshot, Operations operation,
            bool leftOperand, IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.And:
                    return And(snapshot, leftOperand, rightOperand);
                case Operations.Or:
                    return Or(snapshot, leftOperand, rightOperand);
                case Operations.Xor:
                    return Xor(snapshot, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical operation for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left interval operand of logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value Logical<T>(ISnapshotReadWrite snapshot, Operations operation,
            IntervalValue<T> leftOperand, bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Logical(snapshot, operation, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical operation for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="leftOperand">Left interval operand of logical operation.</param>
        /// <param name="rightOperand">Right interval operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value Logical<TLeft, TRight>(ISnapshotReadWrite snapshot, Operations operation,
            IntervalValue<TLeft> leftOperand, IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            switch (operation)
            {
                case Operations.And:
                    return And(snapshot, leftOperand, rightOperand);
                case Operations.Or:
                    return Or(snapshot, leftOperand, rightOperand);
                case Operations.Xor:
                    return Xor(snapshot, leftOperand, rightOperand);
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
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="concreteOperand">One concrete boolean operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value AbstractLogical(ISnapshotReadWrite snapshot, Operations operation,
            bool concreteOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    return AbstractAnd(snapshot, concreteOperand);
                case Operations.Or:
                    return AbstractOr(snapshot, concreteOperand);
                case Operations.Xor:
                    return AbstractXor(snapshot);
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
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="intervalOperand">One specified interval operand of logical operation.</param>
        /// <returns>If operation is logical, it returns boolean result, otherwise <c>null</c>.</returns>
        public static Value AbstractLogical<T>(ISnapshotReadWrite snapshot, Operations operation,
            IntervalValue<T> intervalOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            switch (operation)
            {
                case Operations.And:
                    return AbstractAnd(snapshot, intervalOperand);
                case Operations.Or:
                    return AbstractOr(snapshot, intervalOperand);
                case Operations.Xor:
                    return AbstractXor(snapshot);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Return an abstract boolean result of logical operation when operands are unknown.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <returns>If operation is logical, it returns abstract boolean, otherwise <c>null</c>.</returns>
        public static AnyBooleanValue AbstractLogical(ISnapshotReadWrite snapshot, Operations operation)
        {
            return IsLogical(operation) ? snapshot.AnyBooleanValue : null;
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
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left boolean operand of AND logical operation.</param>
        /// <param name="rightOperand">Right interval operand of AND logical operation.</param>
        /// <returns><c>true</c> whether both operands are <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value And<T>(ISnapshotReadWrite snapshot, bool leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
            {
                return snapshot.CreateBool(leftOperand && convertedValue);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical AND for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left interval operand of AND logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of AND logical operation.</param>
        /// <returns><c>true</c> whether both operands are <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value And<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return And(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical AND for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left interval operand of AND logical operation.</param>
        /// <param name="rightOperand">Right interval operand of AND logical operation.</param>
        /// <returns><c>true</c> whether both operands are <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value And<TLeft, TRight>(ISnapshotReadWrite snapshot, IntervalValue<TLeft> leftOperand,
            IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<TLeft>(leftOperand, out convertedValue))
            {
                return And(snapshot, convertedValue, rightOperand);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical AND for one given boolean operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="concreteOperand">One concrete boolean operand of AND logical operation.</param>
        /// <returns><c>false</c> whether the operand is <c>false</c>, otherwise abstract boolean.</returns>
        public static Value AbstractAnd(ISnapshotReadWrite snapshot, bool concreteOperand)
        {
            if (concreteOperand)
            {
                return snapshot.AnyBooleanValue;
            }
            else
            {
                return snapshot.CreateBool(false);
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
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="intervalOperand">One specified interval operand of AND logical operation.</param>
        /// <returns><c>false</c> whether the operand is <c>false</c>, otherwise abstract boolean.</returns>
        public static Value AbstractAnd<T>(ISnapshotReadWrite snapshot, IntervalValue<T> intervalOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(intervalOperand, out convertedValue))
            {
                return AbstractAnd(snapshot, convertedValue);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        #endregion And

        #region Or

        /// <summary>
        /// Perform logical OR for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left boolean operand of OR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of OR logical operation.</param>
        /// <returns><c>true</c> whether either operand is <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value Or<T>(ISnapshotReadWrite snapshot, bool leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
            {
                return snapshot.CreateBool(leftOperand || convertedValue);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical OR for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left interval operand of OR logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of OR logical operation.</param>
        /// <returns><c>true</c> whether either operand is <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value Or<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Or(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical OR for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left interval operand of OR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of OR logical operation.</param>
        /// <returns><c>true</c> whether either operand is <c>true</c>, otherwise <c>false</c>.</returns>
        public static Value Or<TLeft, TRight>(ISnapshotReadWrite snapshot, IntervalValue<TLeft> leftOperand,
            IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<TLeft>(leftOperand, out convertedValue))
            {
                return Or(snapshot, convertedValue, rightOperand);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical OR for one given boolean operand. The other is unknown.
        /// </summary>
        /// <remarks>
        /// It does not matter whether the concrete operand is on left or right side,
        /// result is the same, because logical operation is commutative.
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="concreteOperand">One concrete boolean operand of OR logical operation.</param>
        /// <returns><c>true</c> whether the operand is <c>true</c>, otherwise abstract boolean.</returns>
        public static Value AbstractOr(ISnapshotReadWrite snapshot, bool concreteOperand)
        {
            if (concreteOperand)
            {
                return snapshot.CreateBool(true);
            }
            else
            {
                return snapshot.AnyBooleanValue;
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
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="intervalOperand">One specified interval operand of OR logical operation.</param>
        /// <returns><c>true</c> whether the operand is <c>true</c>, otherwise abstract boolean.</returns>
        public static Value AbstractOr<T>(ISnapshotReadWrite snapshot, IntervalValue<T> intervalOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(intervalOperand, out convertedValue))
            {
                return AbstractOr(snapshot, convertedValue);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        #endregion Or

        #region Xor

        /// <summary>
        /// Perform logical XOR for given boolean and interval operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left boolean operand of XOR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of XOR logical operation.</param>
        /// <returns><c>true</c> whether operands are not equal, otherwise <c>false</c>.</returns>
        public static Value Xor<T>(ISnapshotReadWrite snapshot, bool leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(rightOperand, out convertedValue))
            {
                return snapshot.CreateBool(leftOperand != convertedValue);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Perform logical XOR for given interval and boolean operands.
        /// </summary>
        /// <typeparam name="T">Type of values in interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left interval operand of XOR logical operation.</param>
        /// <param name="rightOperand">Right boolean operand of XOR logical operation.</param>
        /// <returns><c>true</c> whether operands are not equal, otherwise <c>false</c>.</returns>
        public static Value Xor<T>(ISnapshotReadWrite snapshot, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Xor(snapshot, rightOperand, leftOperand);
        }

        /// <summary>
        /// Perform logical XOR for given interval operands.
        /// </summary>
        /// <typeparam name="TLeft">Type of values in left interval operand.</typeparam>
        /// <typeparam name="TRight">Type of values in right interval operand.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="leftOperand">Left interval operand of XOR logical operation.</param>
        /// <param name="rightOperand">Right interval operand of XOR logical operation.</param>
        /// <returns><c>true</c> whether operands are not equal, otherwise <c>false</c>.</returns>
        public static Value Xor<TLeft, TRight>(ISnapshotReadWrite snapshot, IntervalValue<TLeft> leftOperand,
            IntervalValue<TRight> rightOperand)
            where TLeft : IComparable, IComparable<TLeft>, IEquatable<TLeft>
            where TRight : IComparable, IComparable<TRight>, IEquatable<TRight>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<TLeft>(leftOperand, out convertedValue))
            {
                return Xor(snapshot, convertedValue, rightOperand);
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Return an abstract boolean result of XOR logical operation even if at least one operand is known.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <returns>An abstract boolean.</returns>
        public static AnyBooleanValue AbstractXor(ISnapshotReadWrite snapshot)
        {
            return snapshot.AnyBooleanValue;
        }

        #endregion Xor
    }
}
