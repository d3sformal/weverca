using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class contains methods performing arithmetic operations.
    /// </summary>
    /// <remarks>
    /// The behavior of arithmetic operations with integers is such that if result cannot fit into integer,
    /// it is converted into floating-point number. It can happen when result overflows or underflows
    /// during addition, subtraction or multiplication or when operands are not divisible during division.
    /// The type of result is clear if we calculate with concrete numbers. Result of operations with integer
    /// interval is also interval, but it can contain integers and floating-point numbers too, so
    /// it degrades into entire floating-point interval if there is even just one non-integer. Moreover,
    /// if divisor is (or can be) zero, than it is division by zero that ends with false value. Note that
    /// arithmetic operations, specifically subtraction and division, are not commutative, though we must
    /// provide additional methods with specified order of operands. Modulo operation is evaluated
    /// in separated <see cref="ModuloOperation" /> class.
    /// </remarks>
    public static class ArithmeticOperation
    {
        /// <summary>
        /// The entire integer interval from minimum to maximum value.
        /// </summary>
        private static IntervalValue<int> entireIntegerInterval;

        /// <summary>
        /// Interval of all values converted from a boolean value.
        /// </summary>
        private static IntervalValue<int> booleanInterval;

        /// <summary>
        /// Indicate whether the operation is arithmetic.
        /// </summary>
        /// <param name="operation">Operation to be checked.</param>
        /// <returns><c>true</c> whether operation is arithmetic, otherwise <c>false</c>.</returns>
        public static bool IsArithmetic(Operations operation)
        {
            switch (operation)
            {
                case Operations.Add:
                case Operations.Sub:
                case Operations.Mul:
                case Operations.Div:
                    return true;
                default:
                    return false;
            }
        }

        #region Concrete aritmetic

        /// <summary>
        /// Perform arithmetic operation of given integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static ScalarValue Arithmetic(FlowController flow, Operations operation,
            int leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of given integer and floating-point number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static ScalarValue Arithmetic(FlowController flow, Operations operation,
            int leftOperand, double rightOperand)
        {
            return Arithmetic(flow, operation, TypeConversion.ToFloat(leftOperand), rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point number and integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static ScalarValue Arithmetic(FlowController flow, Operations operation,
            double leftOperand, int rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand, TypeConversion.ToFloat(rightOperand));
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static ScalarValue Arithmetic(FlowController flow, Operations operation,
            double leftOperand, double rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        #region Addition

        /// <summary>
        /// Add integer <paramref name="augend" /> and integer <paramref name="addend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer augend of addition operation.</param>
        /// <param name="addend">Integer addend of addition operation.</param>
        /// <returns>Floating-point result whether sum overflows/underflows, otherwise integer.</returns>
        public static ScalarValue Add(ISnapshotReadWrite snapshot, int augend, int addend)
        {
            // Result of addition can overflow or underflow
            if ((addend >= 0) ? (augend <= int.MaxValue - addend) : (augend >= int.MinValue - addend))
            {
                return snapshot.CreateInt(augend + addend);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return snapshot.CreateDouble(TypeConversion.ToFloat(augend) + addend);
            }
        }

        /// <summary>
        /// Add integer <paramref name="augend" /> and floating-point number <paramref name="addend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer augend of addition operation.</param>
        /// <param name="addend">Floating-point number addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point number.</returns>
        public static FloatValue Add(ISnapshotReadWrite snapshot, int augend, double addend)
        {
            return Add(snapshot, TypeConversion.ToFloat(augend), addend);
        }

        /// <summary>
        /// Add floating-point number <paramref name="augend" /> and integer <paramref name="addend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point number augend of addition operation.</param>
        /// <param name="addend">Integer addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point number.</returns>
        public static FloatValue Add(ISnapshotReadWrite snapshot, double augend, int addend)
        {
            return Add(snapshot, augend, TypeConversion.ToFloat(addend));
        }

        /// <summary>
        /// Add floating-point number <paramref name="augend" /> and <paramref name="addend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point number augend of addition operation.</param>
        /// <param name="addend">Floating-point number addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point number.</returns>
        public static FloatValue Add(ISnapshotReadWrite snapshot, double augend, double addend)
        {
            return snapshot.CreateDouble(augend + addend);
        }

        #endregion Addition

        #region Subtraction

        /// <summary>
        /// Subtract integer <paramref name="subtrahend" /> from integer <paramref name="minuend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer subtrahend of subtraction operation.</param>
        /// <returns>Floating-point result if difference overflows/underflows, otherwise integer.</returns>
        public static ScalarValue Subtract(ISnapshotReadWrite snapshot, int minuend, int subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((subtrahend >= 0) ? (minuend >= int.MinValue + subtrahend)
                : (minuend <= int.MaxValue + subtrahend))
            {
                return snapshot.CreateInt(minuend - subtrahend);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return snapshot.CreateDouble(TypeConversion.ToFloat(minuend) - subtrahend);
            }
        }

        /// <summary>
        /// Subtract floating-point number subtrahend from integer minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point number subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point number.</returns>
        public static FloatValue Subtract(ISnapshotReadWrite snapshot, int minuend, double subtrahend)
        {
            return Subtract(snapshot, TypeConversion.ToFloat(minuend), subtrahend);
        }

        /// <summary>
        /// Subtract integer subtrahend from floating-point number minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point number minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point number.</returns>
        public static FloatValue Subtract(ISnapshotReadWrite snapshot, double minuend, int subtrahend)
        {
            return Subtract(snapshot, minuend, TypeConversion.ToFloat(subtrahend));
        }

        /// <summary>
        /// Subtract floating-point number <paramref name="subtrahend" /> from <paramref name="minuend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point number minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point number subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point number.</returns>
        public static FloatValue Subtract(ISnapshotReadWrite snapshot, double minuend, double subtrahend)
        {
            return snapshot.CreateDouble(minuend - subtrahend);
        }

        #endregion Subtraction

        #region Multiplication

        /// <summary>
        /// Multiply integer <paramref name="multiplicand" /> and integer <paramref name="multiplier" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Integer multiplier of multiplication operation.</param>
        /// <returns>Floating-point result whether product overflows/underflows, otherwise integer.</returns>
        public static ScalarValue Multiply(ISnapshotReadWrite snapshot, int multiplicand, int multiplier)
        {
            // Result of multiplication can overflow or underflow
            if ((multiplier == 0) || (((multiplicand >= 0) == (multiplier >= 0))
                ? (multiplicand <= int.MaxValue / multiplier)
                : (multiplicand <= int.MinValue / multiplier)))
            {
                return snapshot.CreateInt(multiplicand * multiplier);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return snapshot.CreateDouble(TypeConversion.ToFloat(multiplicand) * multiplier);
            }
        }

        /// <summary>
        /// Multiply integer multiplicand and floating-point number multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Floating-point number multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point number.</returns>
        public static FloatValue Multiply(ISnapshotReadWrite snapshot, int multiplicand, double multiplier)
        {
            return Multiply(snapshot, TypeConversion.ToFloat(multiplicand), multiplier);
        }

        /// <summary>
        /// Multiply floating-point number multiplicand and integer multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point number multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Integer multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point number.</returns>
        public static FloatValue Multiply(ISnapshotReadWrite snapshot, double multiplicand, int multiplier)
        {
            return Multiply(snapshot, multiplicand, TypeConversion.ToFloat(multiplier));
        }

        /// <summary>
        /// Multiply floating-point number multiplicand and multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point number multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Floating-point number multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point number.</returns>
        public static FloatValue Multiply(ISnapshotReadWrite snapshot, double multiplicand, double multiplier)
        {
            return snapshot.CreateDouble(multiplicand * multiplier);
        }

        #endregion Multiplication

        #region Division

        /// <summary>
        /// Divide integer <paramref name="dividend" /> by integer <paramref name="divisor" />.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of division operation.</param>
        /// <param name="divisor">Integer divisor of division operation.</param>
        /// <returns>Floating-point result whether operands are not divisible, otherwise integer.</returns>
        public static ScalarValue Divide(FlowController flow, int dividend, int divisor)
        {
            if (divisor != 0)
            {
                // Not divisible numbers result to floating-point number.
                // There is one case when result should be an integer,
                // but it is a floating-point number: Minimal integer divided by -1.
                if (((dividend % divisor) == 0) && ((dividend != int.MinValue) || (divisor != -1)))
                {
                    return flow.OutSet.CreateInt(dividend / divisor);
                }
                else
                {
                    return flow.OutSet.CreateDouble(TypeConversion.ToFloat(dividend) / divisor);
                }
            }
            else
            {
                return WarnDivideByZero(flow);
            }
        }

        /// <summary>
        /// Divide integer <paramref name="dividend" /> by floating-point number <paramref name="divisor" />.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of division operation.</param>
        /// <param name="divisor">Floating-point number divisor of division operation.</param>
        /// <returns>Quotient of both operands as a floating-point number.</returns>
        public static ScalarValue Divide(FlowController flow, int dividend, double divisor)
        {
            return Divide(flow, TypeConversion.ToFloat(dividend), divisor);
        }

        /// <summary>
        /// Divide floating-point number <paramref name="dividend" /> by integer <paramref name="divisor" />.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point number dividend of division operation.</param>
        /// <param name="divisor">Integer divisor of division operation.</param>
        /// <returns>Quotient of both operands as a floating-point number.</returns>
        public static ScalarValue Divide(FlowController flow, double dividend, int divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloat(divisor));
        }

        /// <summary>
        /// Divide floating-point number <paramref name="dividend" /> by <paramref name="divisor" />.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point number dividend of division operation.</param>
        /// <param name="divisor">Floating-point number divisor of division operation.</param>
        /// <returns>Quotient of both operands as a floating-point number.</returns>
        public static ScalarValue Divide(FlowController flow, double dividend, double divisor)
        {
            if (divisor != 0.0)
            {
                return flow.OutSet.CreateDouble(dividend / divisor);
            }
            else
            {
                return WarnDivideByZero(flow);
            }
        }

        #endregion Division

        #endregion Concrete aritmetic

        #region Left concrete and right abstract operand aritmetic

        /// <summary>
        /// Perform arithmetic operation of given integer number and interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            int leftOperand, IntervalValue<int> rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of given integer number and floating-point interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            int leftOperand, IntervalValue<double> rightOperand)
        {
            return Arithmetic(flow, operation, TypeConversion.ToFloat(leftOperand), rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point number and integer interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            double leftOperand, IntervalValue<int> rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand,
                TypeConversion.ToFloatInterval(flow.OutSet, rightOperand));
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point number and interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            double leftOperand, IntervalValue<double> rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of given concrete and abstract integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, int leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point number and abstract integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, double leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of given integer and abstract boolean operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, int leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point number and abstract boolean operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, double leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        #region Addition

        /// <summary>
        /// Add integer <paramref name="augend" /> and integer interval <paramref name="addend" />.
        /// </summary>
        /// <remarks>
        /// Arithmetic of integers has a specific behavior in PHP language. If result of arithmetic
        /// operation overflow or underflow, it is converted into floating-point number. It can cause
        /// problems in operations like widening, where the result can be integer interval with extreme
        /// endpoints, i.e. maximal or minimal integer values. Every increment or decrement than causes
        /// overflow or underflow.
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer augend of addition operation.</param>
        /// <param name="addend">Integer interval addend of addition operation.</param>
        /// <returns>Floating-point interval whether sum overflows, otherwise integer interval.</returns>
        public static Value Add(ISnapshotReadWrite snapshot, int augend, IntervalValue<int> addend)
        {
            // Result of addition can overflow or underflow
            if ((augend >= 0) ? (addend.End <= int.MaxValue - augend)
                : (addend.Start >= int.MinValue - augend))
            {
                return snapshot.CreateIntegerInterval(augend + addend.Start, augend + addend.End);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Add(snapshot, TypeConversion.ToFloat(augend),
                    TypeConversion.ToFloatInterval(snapshot, addend));
            }
        }

        /// <summary>
        /// Add integer augend and floating-point interval addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer augend of addition operation.</param>
        /// <param name="addend">Floating-point interval addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot, int augend,
            IntervalValue<double> addend)
        {
            return Add(snapshot, TypeConversion.ToFloat(augend), addend);
        }

        /// <summary>
        /// Add floating-point number augend and integer interval addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point number augend of addition operation.</param>
        /// <param name="addend">Integer interval addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot, double augend,
            IntervalValue<int> addend)
        {
            return Add(snapshot, augend, TypeConversion.ToFloatInterval(snapshot, addend));
        }

        /// <summary>
        /// Add floating-point number augend and floating-point interval addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point number augend of addition operation.</param>
        /// <param name="addend">Floating-point interval addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot, double augend,
            IntervalValue<double> addend)
        {
            return snapshot.CreateFloatInterval(augend + addend.Start, augend + addend.End);
        }

        #endregion Addition

        #region Subtraction

        /// <summary>
        /// Subtract integer interval subtrahend from integer minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer interval subtrahend of subtraction operation.</param>
        /// <returns>Floating-point interval if difference underflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Subtract(ISnapshotReadWrite snapshot, int minuend, IntervalValue<int> subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((minuend >= 0) ? (subtrahend.Start >= minuend - int.MaxValue)
                : (subtrahend.End <= minuend - int.MinValue))
            {
                return snapshot.CreateIntegerInterval(minuend - subtrahend.End, minuend - subtrahend.Start);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Subtract(snapshot, TypeConversion.ToFloat(minuend),
                    TypeConversion.ToFloatInterval(snapshot, subtrahend));
            }
        }

        /// <summary>
        /// Subtract floating-point interval subtrahend from integer minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot,
            int minuend, IntervalValue<double> subtrahend)
        {
            return Subtract(snapshot, TypeConversion.ToFloat(minuend), subtrahend);
        }

        /// <summary>
        /// Subtract integer interval subtrahend from floating-point number minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point number minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot, double minuend,
            IntervalValue<int> subtrahend)
        {
            return Subtract(snapshot, minuend, TypeConversion.ToFloatInterval(snapshot, subtrahend));
        }

        /// <summary>
        /// Subtract floating-point interval subtrahend from floating-point number minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point number minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot, double minuend,
            IntervalValue<double> subtrahend)
        {
            return snapshot.CreateFloatInterval(minuend - subtrahend.End, minuend - subtrahend.Start);
        }

        #endregion Subtraction

        #region Multiplication

        /// <summary>
        /// Multiply integer multiplicand and integer interval multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Integer interval multiplier of multiplication operation.</param>
        /// <returns>Floating-point interval whether product overflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Multiply(ISnapshotReadWrite snapshot, int multiplicand,
            IntervalValue<int> multiplier)
        {
            // Result of multiplication can underflow or underflow
            var isMultiplicandNonNegative = multiplicand >= 0.0;
            if ((multiplicand == 0.0) || (isMultiplicandNonNegative
                ? ((multiplier.Start >= 0.0) ? (multiplier.End <= int.MaxValue / multiplicand)
                : ((multiplier.Start >= int.MinValue / multiplicand)
                && ((multiplier.End < 0.0) || (multiplier.End <= int.MaxValue / multiplicand))))
                : ((multiplier.Start >= 0.0) ? (multiplicand >= int.MinValue / multiplier.End)
                : ((multiplier.Start >= int.MaxValue / multiplicand)
                && ((multiplier.End < 0.0) || (multiplicand >= int.MinValue / multiplier.End))))))
            {
                // When multiplicand is negative, interval is reversed and endpoints swap
                if (isMultiplicandNonNegative)
                {
                    return snapshot.CreateIntegerInterval(multiplicand * multiplier.Start,
                        multiplicand * multiplier.End);
                }
                else
                {
                    return snapshot.CreateIntegerInterval(multiplicand * multiplier.End,
                        multiplicand * multiplier.Start);
                }
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Multiply(snapshot, TypeConversion.ToFloat(multiplicand),
                    TypeConversion.ToFloatInterval(snapshot, multiplier));
            }
        }

        /// <summary>
        /// Multiply integer multiplicand and floating-point interval multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Floating-point interval multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot, int multiplicand,
            IntervalValue<double> multiplier)
        {
            return Multiply(snapshot, TypeConversion.ToFloat(multiplicand), multiplier);
        }

        /// <summary>
        /// Multiply floating-point number multiplicand and integer interval multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point number multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Integer interval multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot, double multiplicand,
            IntervalValue<int> multiplier)
        {
            return Multiply(snapshot, multiplicand, TypeConversion.ToFloatInterval(snapshot, multiplier));
        }

        /// <summary>
        /// Multiply floating-point number multiplicand and floating-point interval multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point number multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Floating-point interval multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot, double multiplicand,
            IntervalValue<double> multiplier)
        {
            // When multiplicand is negative, interval is reversed and endpoints swap
            if (multiplicand >= 0.0)
            {
                return snapshot.CreateFloatInterval(multiplicand * multiplier.Start,
                    multiplicand * multiplier.End);
            }
            else
            {
                return snapshot.CreateFloatInterval(multiplicand * multiplier.End,
                    multiplicand * multiplier.Start);
            }
        }

        #endregion Multiplication

        #region Division

        /// <summary>
        /// Divide integer <paramref name="dividend" /> by integer interval <paramref name="divisor" />.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of division operation.</param>
        /// <param name="divisor">Integer interval divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, int dividend, IntervalValue<int> divisor)
        {
            // Not divisible numbers result to floating-point number.
            // Unfortunately, except for trivial cases, the result after division
            // is always an interval mixed of integers and floating-point numbers.
            return Divide(flow, TypeConversion.ToFloat(dividend),
                TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

        /// <summary>
        /// Divide integer dividend by floating-point interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of division operation.</param>
        /// <param name="divisor">Floating-point interval divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, int dividend,
            IntervalValue<double> divisor)
        {
            return Divide(flow, TypeConversion.ToFloat(dividend), divisor);
        }

        /// <summary>
        /// Divide floating-point number dividend by integer interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point number dividend of division operation.</param>
        /// <param name="divisor">Integer interval divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, double dividend,
            IntervalValue<int> divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

        /// <summary>
        /// Divide floating-point number dividend by floating-point interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point number dividend of division operation.</param>
        /// <param name="divisor">Floating-point interval divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, double dividend,
            IntervalValue<double> divisor)
        {
            if ((divisor.Start > 0.0) || (divisor.End < 0.0))
            {
                if (dividend >= 0.0)
                {
                    return flow.OutSet.CreateFloatInterval(dividend / divisor.End, dividend / divisor.Start);
                }
                else
                {
                    return flow.OutSet.CreateFloatInterval(dividend / divisor.Start, dividend / divisor.End);
                }
            }
            else
            {
                return WarnPossibleDivideByZero(flow);
            }
        }

        #endregion Division

        #endregion Left concrete and right abstract operand aritmetic

        #region Left abstract and right concrete operand aritmetic

        /// <summary>
        /// Perform arithmetic operation of given integer interval and number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<int> leftOperand, int rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of given integer interval and floating-point number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<int> leftOperand, double rightOperand)
        {
            return Arithmetic(flow, operation,
                TypeConversion.ToFloatInterval(flow.OutSet, leftOperand), rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point interval and integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<double> leftOperand, int rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand, TypeConversion.ToFloat(rightOperand));
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point interval and number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<double> leftOperand, double rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of abstract and given concrete integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, int rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract integer and given floating-point number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, double rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract boolean and given integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right integer operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, int rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract boolean and given floating-point number operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, double rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        #region Addition

        /// <summary>
        /// Add integer interval <paramref name="augend" /> and integer <paramref name="addend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer interval augend of addition operation.</param>
        /// <param name="addend">Integer addend of addition operation.</param>
        /// <returns>Floating-point interval whether sum overflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Add(ISnapshotReadWrite snapshot, IntervalValue<int> augend, int addend)
        {
            return Add(snapshot, addend, augend);
        }

        /// <summary>
        /// Add integer interval augend and floating-point number addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer interval augend of addition operation.</param>
        /// <param name="addend">Floating-point number addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot,
            IntervalValue<int> augend, double addend)
        {
            return Add(snapshot, addend, augend);
        }

        /// <summary>
        /// Add floating-point interval augend and integer addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point interval augend of addition operation.</param>
        /// <param name="addend">Integer addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot,
            IntervalValue<double> augend, int addend)
        {
            return Add(snapshot, addend, augend);
        }

        /// <summary>
        /// Add floating-point interval augend and floating-point number addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point interval augend of addition operation.</param>
        /// <param name="addend">Floating-point number addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot,
            IntervalValue<double> augend, double addend)
        {
            return Add(snapshot, addend, augend);
        }

        #endregion Addition

        #region Subtraction

        /// <summary>
        /// Subtract integer subtrahend from integer interval minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer subtrahend of subtraction operation.</param>
        /// <returns>Floating-point interval if difference underflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Subtract(ISnapshotReadWrite snapshot, IntervalValue<int> minuend, int subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((subtrahend >= 0) ? (minuend.Start >= int.MinValue + subtrahend)
                : (minuend.End <= int.MaxValue + subtrahend))
            {
                return snapshot.CreateIntegerInterval(minuend.Start - subtrahend, minuend.End - subtrahend);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Subtract(snapshot, TypeConversion.ToFloatInterval(snapshot, minuend),
                    TypeConversion.ToFloat(subtrahend));
            }
        }

        /// <summary>
        /// Subtract floating-point number subtrahend from integer interval minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point number subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot,
            IntervalValue<int> minuend, double subtrahend)
        {
            return Subtract(snapshot, TypeConversion.ToFloatInterval(snapshot, minuend), subtrahend);
        }

        /// <summary>
        /// Subtract integer subtrahend from floating-point interval minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot,
            IntervalValue<double> minuend, int subtrahend)
        {
            return Subtract(snapshot, minuend, TypeConversion.ToFloat(subtrahend));
        }

        /// <summary>
        /// Subtract floating-point number subtrahend from floating-point interval minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point number minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot,
            IntervalValue<double> minuend, double subtrahend)
        {
            return snapshot.CreateFloatInterval(minuend.Start - subtrahend, minuend.End - subtrahend);
        }

        #endregion Subtraction

        #region Multiplication

        /// <summary>
        /// Multiply integer interval multiplicand and integer multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer interval multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Integer multiplier of multiplication operation.</param>
        /// <returns>Floating-point interval whether product overflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Multiply(ISnapshotReadWrite snapshot, IntervalValue<int> multiplicand,
            int multiplier)
        {
            return Multiply(snapshot, multiplier, multiplicand);
        }

        /// <summary>
        /// Multiply integer interval multiplicand and floating-point number multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer interval multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Floating-point number multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot,
            IntervalValue<int> multiplicand, double multiplier)
        {
            return Multiply(snapshot, multiplier, multiplicand);
        }

        /// <summary>
        /// Multiply floating-point interval multiplicand and integer number multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point interval multiplicand of multiplication.</param>
        /// <param name="multiplier">Integer number multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot,
            IntervalValue<double> multiplicand, int multiplier)
        {
            return Multiply(snapshot, multiplier, multiplicand);
        }

        /// <summary>
        /// Multiply floating-point interval multiplicand and floating-point number multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point interval multiplicand of multiplication.</param>
        /// <param name="multiplier">Floating-point number multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot,
            IntervalValue<double> multiplicand, double multiplier)
        {
            return Multiply(snapshot, multiplier, multiplicand);
        }

        #endregion Multiplication

        #region Division

        /// <summary>
        /// Divide integer interval <paramref name="dividend" /> by integer <paramref name="divisor" />.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of division operation.</param>
        /// <param name="divisor">Integer divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, IntervalValue<int> dividend, int divisor)
        {
            // Not divisible numbers result to floating-point number.
            // Unfortunately, except for trivial cases, the result after division
            // is always an interval mixed of integers and floating-point numbers.
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend),
                TypeConversion.ToFloat(divisor));
        }

        /// <summary>
        /// Divide integer interval dividend by floating-point number divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of division operation.</param>
        /// <param name="divisor">Floating-point number divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, IntervalValue<int> dividend, double divisor)
        {
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend), divisor);
        }

        /// <summary>
        /// Divide floating-point interval dividend by integer divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of division operation.</param>
        /// <param name="divisor">Integer divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, IntervalValue<double> dividend, int divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloat(divisor));
        }

        /// <summary>
        /// Divide floating-point interval dividend by floating-point number divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of division operation.</param>
        /// <param name="divisor">Floating-point number divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, IntervalValue<double> dividend, double divisor)
        {
            if (divisor != 0.0)
            {
                if (divisor >= 0.0)
                {
                    return flow.OutSet.CreateFloatInterval(dividend.Start / divisor, dividend.End / divisor);
                }
                else
                {
                    return flow.OutSet.CreateFloatInterval(dividend.End / divisor, dividend.Start / divisor);
                }
            }
            else
            {
                return WarnDivideByZero(flow);
            }
        }

        #endregion Division

        #endregion Left abstract and right concrete operand aritmetic

        #region Abstract aritmetic

        /// <summary>
        /// Perform arithmetic operation of given integer interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<int> leftOperand, IntervalValue<int> rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of given integer and floating-point interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<int> leftOperand, IntervalValue<double> rightOperand)
        {
            return Arithmetic(flow, operation,
                TypeConversion.ToFloatInterval(flow.OutSet, leftOperand), rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point and integer interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<double> leftOperand, IntervalValue<int> rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand,
                TypeConversion.ToFloatInterval(flow.OutSet, rightOperand));
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point interval operand of arithmetic operation.</param>
        /// <param name="rightOperand">Right floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<double> leftOperand, IntervalValue<double> rightOperand)
        {
            switch (operation)
            {
                case Operations.Add:
                    return Add(flow.OutSet, leftOperand, rightOperand);
                case Operations.Sub:
                    return Subtract(flow.OutSet, leftOperand, rightOperand);
                case Operations.Mul:
                    return Multiply(flow.OutSet, leftOperand, rightOperand);
                case Operations.Div:
                    return Divide(flow, leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform arithmetic operation of given integer interval and abstract integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point interval and abstract integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point number operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract integer and given integer interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract integer and given floating-point interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of given integer interval and abstract boolean operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of given floating-point interval and abstract boolean operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="leftOperand">Left floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of left abstract integer and right abstract boolean operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value RightAbstractBooleanArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, booleanInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract boolean and given integer interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right integer interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of abstract boolean and given floating-point interval operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <param name="rightOperand">Right floating-point interval operand of arithmetic operation.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        /// <summary>
        /// Perform arithmetic operation of left abstract boolean and right abstract integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value LeftAbstractBooleanArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, entireIntegerInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of two abstract boolean operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value AbstractBooleanArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, booleanInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of two abstract integer operands.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <returns>If operation is arithmetic, it returns its result, otherwise <c>null</c>.</returns>
        public static Value AbstractIntegerArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, entireIntegerInterval);
        }

        /// <summary>
        /// Perform arithmetic operation of two abstract floating-point number operands.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="operation">Operation to be performed, only the arithmetic one evaluates.</param>
        /// <returns>If operation is arithmetic, an abstract floating-point, otherwise <c>null</c>.</returns>
        public static AnyFloatValue AbstractFloatArithmetic(SnapshotBase snapshot, Operations operation)
        {
            return IsArithmetic(operation) ? snapshot.AnyFloatValue : null;
        }

        #region Addition

        /// <summary>
        /// Add integer interval <paramref name="augend" /> and <paramref name="addend" /> of the same type.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer interval augend of addition operation.</param>
        /// <param name="addend">Integer interval addend of addition operation.</param>
        /// <returns>Floating-point interval whether sum overflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Add(ISnapshotReadWrite snapshot, IntervalValue<int> augend,
            IntervalValue<int> addend)
        {
            // Result of addition can overflow or underflow
            if ((augend.Start >= 0) ? (addend.End <= int.MaxValue - augend.End)
                : ((addend.Start >= int.MinValue - augend.Start)
                && ((augend.End < 0) || (addend.End <= int.MaxValue - augend.End))))
            {
                return snapshot.CreateIntegerInterval(augend.Start + addend.Start, augend.Start + addend.End);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Add(snapshot, TypeConversion.ToFloatInterval(snapshot, augend),
                    TypeConversion.ToFloatInterval(snapshot, addend));
            }
        }

        /// <summary>
        /// Add integer interval augend and floating-point interval addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Integer interval augend of addition operation.</param>
        /// <param name="addend">Floating-point interval addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot, IntervalValue<int> augend,
            IntervalValue<double> addend)
        {
            return Add(snapshot, TypeConversion.ToFloatInterval(snapshot, augend), addend);
        }

        /// <summary>
        /// Add floating-point interval augend and integer interval addend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point interval augend of addition operation.</param>
        /// <param name="addend">Integer interval addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot, IntervalValue<double> augend,
            IntervalValue<int> addend)
        {
            return Add(snapshot, augend, TypeConversion.ToFloatInterval(snapshot, addend));
        }

        /// <summary>
        /// Add floating-point interval <paramref name="augend" /> and <paramref name="addend" />.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="augend">Floating-point interval augend of addition operation.</param>
        /// <param name="addend">Floating-point interval addend of addition operation.</param>
        /// <returns>Sum of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Add(ISnapshotReadWrite snapshot, IntervalValue<double> augend,
            IntervalValue<double> addend)
        {
            return snapshot.CreateFloatInterval(augend.Start + addend.Start, augend.End + addend.End);
        }

        #endregion Addition

        #region Subtraction

        /// <summary>
        /// Subtract integer interval subtrahend from minuend of the same type.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer subtrahend of subtraction operation.</param>
        /// <returns>Floating-point interval if difference underflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Subtract(ISnapshotReadWrite snapshot, IntervalValue<int> minuend,
            IntervalValue<int> subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((minuend.Start >= 0) ? (subtrahend.Start >= minuend.End - int.MaxValue)
                : ((subtrahend.End <= minuend.Start - int.MinValue)
                && ((minuend.End < 0) || (subtrahend.Start >= minuend.End - int.MaxValue))))
            {
                return snapshot.CreateIntegerInterval(minuend.Start - subtrahend.End,
                    minuend.End - subtrahend.Start);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Subtract(snapshot, TypeConversion.ToFloatInterval(snapshot, minuend),
                    TypeConversion.ToFloatInterval(snapshot, subtrahend));
            }
        }

        /// <summary>
        /// Subtract floating-point interval subtrahend from integer interval minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Integer interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot, IntervalValue<int> minuend,
            IntervalValue<double> subtrahend)
        {
            return Subtract(snapshot, TypeConversion.ToFloatInterval(snapshot, minuend), subtrahend);
        }

        /// <summary>
        /// Subtract integer interval subtrahend from floating-point interval minuend.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Integer interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot,
            IntervalValue<double> minuend, IntervalValue<int> subtrahend)
        {
            return Subtract(snapshot, minuend, TypeConversion.ToFloatInterval(snapshot, subtrahend));
        }

        /// <summary>
        /// Subtract floating-point interval subtrahend from minuend of the same type.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="minuend">Floating-point interval minuend of subtraction operation.</param>
        /// <param name="subtrahend">Floating-point interval subtrahend of subtraction operation.</param>
        /// <returns>Difference of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Subtract(ISnapshotReadWrite snapshot,
            IntervalValue<double> minuend, IntervalValue<double> subtrahend)
        {
            return snapshot.CreateFloatInterval(minuend.Start - subtrahend.End,
                minuend.End - subtrahend.Start);
        }

        #endregion Subtraction

        #region Multiplication

        /// <summary>
        /// Multiply integer interval multiplicand and multiplier of the same type.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer interval multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Integer interval multiplier of multiplication operation.</param>
        /// <returns>Floating-point interval whether product overflows, otherwise integer interval.</returns>
        /// <seealso cref="Add(ISnapshotReadWrite, int, IntervalValue{int})" />
        public static Value Multiply(ISnapshotReadWrite snapshot, IntervalValue<int> multiplicand,
            IntervalValue<int> multiplier)
        {
            // TODO: Calculate more precise result
            return snapshot.AnyValue;
        }

        /// <summary>
        /// Multiply integer interval multiplicand and floating-point interval multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Integer interval multiplicand of multiplication operation.</param>
        /// <param name="multiplier">Floating-point interval multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot,
            IntervalValue<int> multiplicand, IntervalValue<double> multiplier)
        {
            return Multiply(snapshot, TypeConversion.ToFloatInterval(snapshot, multiplicand), multiplier);
        }

        /// <summary>
        /// Multiply floating-point interval multiplicand and integer interval multiplier.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point interval multiplicand of multiplication.</param>
        /// <param name="multiplier">Integer interval multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot,
            IntervalValue<double> multiplicand, IntervalValue<int> multiplier)
        {
            return Multiply(snapshot, multiplicand, TypeConversion.ToFloatInterval(snapshot, multiplier));
        }

        /// <summary>
        /// Multiply floating-point interval multiplicand and multiplier of the same type.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="multiplicand">Floating-point interval multiplicand of multiplication.</param>
        /// <param name="multiplier">Floating-point interval multiplier of multiplication operation.</param>
        /// <returns>Product of both operands as a floating-point interval.</returns>
        public static IntervalValue<double> Multiply(ISnapshotReadWrite snapshot,
            IntervalValue<double> multiplicand, IntervalValue<double> multiplier)
        {
            // TODO: Calculate more precise result
            return snapshot.CreateFloatInterval(double.MinValue, double.MaxValue);
        }

        #endregion Multiplication

        #region Division

        /// <summary>
        /// Divide integer interval dividend by divisor of the same type.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of division operation.</param>
        /// <param name="divisor">Integer interval divisor of division operation.</param>
        /// <returns>Quotient as a floating-point interval or false when division by zero.</returns>
        public static Value Divide(FlowController flow, IntervalValue<int> dividend,
            IntervalValue<int> divisor)
        {
            // Not divisible numbers result to floating-point number.
            // Unfortunately, except for trivial cases, the result after division
            // is always an interval mixed of integers and floating-point numbers.
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend),
                TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

        /// <summary>
        /// Divide integer interval dividend by floating-point interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of division operation.</param>
        /// <param name="divisor">Floating-point interval divisor of division operation.</param>
        /// <returns>Quotient of both operands as a floating-point interval.</returns>
        public static Value Divide(FlowController flow, IntervalValue<int> dividend,
            IntervalValue<double> divisor)
        {
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend), divisor);
        }

        /// <summary>
        /// Divide floating-point interval dividend by integer interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of division operation.</param>
        /// <param name="divisor">Integer interval divisor of division operation.</param>
        /// <returns>Quotient of both operands as a floating-point interval.</returns>
        public static Value Divide(FlowController flow, IntervalValue<double> dividend,
            IntervalValue<int> divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

        /// <summary>
        /// Divide floating-point interval dividend by divisor of the same type.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of division operation.</param>
        /// <param name="divisor">Floating-point interval divisor of division operation.</param>
        /// <returns>Quotient of both operands as a floating-point interval.</returns>
        public static Value Divide(FlowController flow, IntervalValue<double> dividend,
            IntervalValue<double> divisor)
        {
            if ((divisor.Start > 0.0) || (divisor.End < 0.0))
            {
                if (divisor.Start > 0.0)
                {
                    if (dividend.Start >= 0.0)
                    {
                        return flow.OutSet.CreateFloatInterval(dividend.Start / divisor.End,
                            dividend.End / divisor.Start);
                    }
                    else if (dividend.End < 0.0)
                    {
                        return flow.OutSet.CreateFloatInterval(dividend.Start / divisor.Start,
                            dividend.End / divisor.End);
                    }
                    else
                    {
                        return flow.OutSet.CreateFloatInterval(dividend.Start / divisor.Start,
                            dividend.End / divisor.Start);
                    }
                }
                else
                {
                    if (dividend.Start >= 0.0)
                    {
                        return flow.OutSet.CreateFloatInterval(dividend.End / divisor.End,
                        dividend.Start / divisor.Start);
                    }
                    else if (dividend.End < 0.0)
                    {
                        return flow.OutSet.CreateFloatInterval(dividend.End / divisor.Start,
                            dividend.Start / divisor.End);
                    }
                    else
                    {
                        return flow.OutSet.CreateFloatInterval(dividend.End / divisor.End,
                            dividend.Start / divisor.End);
                    }
                }
            }
            else
            {
                return WarnPossibleDivideByZero(flow);
            }
        }

        #endregion Division

        #endregion Abstract aritmetic

        /// <summary>
        /// Warn that division by null has occurred and return result of operation, <c>false</c> value.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <returns><c>false</c> value that is result of division by zero.</returns>
        public static BooleanValue DivisionByNull(FlowController flow)
        {
            SetWarning(flow, "Division by zero (converted from null)",
                AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division by null returns false boolean value
            return flow.OutSet.CreateBool(false);
        }

        #region Helper methods

        /// <summary>
        /// Initialize internal class static members like entire integer or boolean interval.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        private static void InitalizeInternals(ISnapshotReadWrite snapshot)
        {
            if (entireIntegerInterval == null)
            {
                entireIntegerInterval = TypeConversion.AnyIntegerToIntegerInterval(snapshot);
                Debug.Assert(booleanInterval == null, "All private fields are initialized together");
                booleanInterval = TypeConversion.AnyBooleanToIntegerInterval(snapshot);
            }
        }

        /// <summary>
        /// Warn that division by zero has occurred and return result of operation, <c>false</c> value.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <returns><c>false</c> value that is result of division by zero.</returns>
        private static BooleanValue WarnDivideByZero(FlowController flow)
        {
            SetWarning(flow, "Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division by zero returns false boolean value
            // Division by floating-point zero does not return NaN or infinite, but false boolean value too
            return flow.OutSet.CreateBool(false);
        }

        /// <summary>
        /// Warn that division by zero has possibly occurred.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <returns>An abstract value because result of operation can be number or <c>false</c>.</returns>
        private static AnyValue WarnPossibleDivideByZero(FlowController flow)
        {
            // As right operant can be range of values, can possibly be 0 too
            // That causes division by zero and returns false
            SetWarning(flow, "Division by any number, possible division by zero",
                AnalysisWarningCause.DIVISION_BY_ZERO);
            return flow.OutSet.AnyValue;
        }

        /// <summary>
        /// Report a warning for the position of current expression.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="message">Message of the warning.</param>
        /// <param name="cause">Cause of the warning.</param>
        private static void SetWarning(FlowController flow, string message, AnalysisWarningCause cause)
        {
            var warning = new AnalysisWarning(flow.CurrentScript.FullName, message,
                flow.CurrentPartial, flow.CurrentProgramPoint, cause);
            AnalysisWarningHandler.SetWarning(flow.OutSet, warning);
        }

        #endregion Helper methods
    }
}
