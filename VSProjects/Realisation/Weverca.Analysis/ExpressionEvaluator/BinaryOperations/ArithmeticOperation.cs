using PHP.Core;
using PHP.Core.AST;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    public static class ArithmeticOperation
    {
        /// <summary>
        /// The entire integer interval from minimum to maximum value
        /// </summary>
        private static IntervalValue<int> entireIntegerInterval;

        /// <summary>
        /// Interval of all values converted from a boolean value
        /// </summary>
        private static IntervalValue<int> booleanInterval;

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

        public static ScalarValue Arithmetic(FlowController flow, Operations operation,
            int leftOperand, double rightOperand)
        {
            return Arithmetic(flow, operation, TypeConversion.ToFloat(leftOperand), rightOperand);
        }

        public static ScalarValue Arithmetic(FlowController flow, Operations operation,
            double leftOperand, int rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand, TypeConversion.ToFloat(rightOperand));
        }

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

        public static ScalarValue Add(FlowOutputSet outset, int augend, int addend)
        {
            // Result of addition can overflow or underflow
            if ((addend >= 0) ? (augend <= int.MaxValue - addend) : (augend >= int.MinValue - addend))
            {
                return outset.CreateInt(augend + addend);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return outset.CreateDouble(TypeConversion.ToFloat(augend) + addend);
            }
        }

        public static FloatValue Add(FlowOutputSet outset, int augend, double addend)
        {
            return Add(outset, TypeConversion.ToFloat(augend), addend);
        }

        public static FloatValue Add(FlowOutputSet outset, double augend, int addend)
        {
            return Add(outset, augend, TypeConversion.ToFloat(addend));
        }

        public static FloatValue Add(FlowOutputSet outset, double augend, double addend)
        {
            return outset.CreateDouble(augend + addend);
        }

        #endregion Addition

        #region Subtraction

        public static ScalarValue Subtract(FlowOutputSet outset, int minuend, int subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((subtrahend >= 0) ? (minuend >= int.MinValue + subtrahend)
                : (minuend <= int.MaxValue + subtrahend))
            {
                return outset.CreateInt(minuend - subtrahend);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return outset.CreateDouble(TypeConversion.ToFloat(minuend) - subtrahend);
            }
        }

        public static FloatValue Subtract(FlowOutputSet outset, int minuend, double subtrahend)
        {
            return Subtract(outset, TypeConversion.ToFloat(minuend), subtrahend);
        }

        public static FloatValue Subtract(FlowOutputSet outset, double minuend, int subtrahend)
        {
            return Subtract(outset, minuend, TypeConversion.ToFloat(subtrahend));
        }

        public static FloatValue Subtract(FlowOutputSet outset, double minuend, double subtrahend)
        {
            return outset.CreateDouble(minuend - subtrahend);
        }

        #endregion Subtraction

        #region Multiplication

        public static ScalarValue Multiply(FlowOutputSet outset, int multiplicand, int multiplier)
        {
            // Result of multiplication can overflow or underflow
            if ((multiplier == 0) || (((multiplicand >= 0) == (multiplier >= 0))
                ? (multiplicand <= int.MaxValue / multiplier)
                : (multiplicand <= int.MinValue / multiplier)))
            {
                return outset.CreateInt(multiplicand * multiplier);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return outset.CreateDouble(TypeConversion.ToFloat(multiplicand) * multiplier);
            }
        }

        public static FloatValue Multiply(FlowOutputSet outset, int multiplicand, double multiplier)
        {
            return Multiply(outset, TypeConversion.ToFloat(multiplicand), multiplier);
        }

        public static FloatValue Multiply(FlowOutputSet outset, double multiplicand, int multiplier)
        {
            return Multiply(outset, multiplicand, TypeConversion.ToFloat(multiplier));
        }

        public static FloatValue Multiply(FlowOutputSet outset, double multiplicand, double multiplier)
        {
            return outset.CreateDouble(multiplicand * multiplier);
        }

        #endregion Multiplication

        #region Division

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

        public static ScalarValue Divide(FlowController flow, int dividend, double divisor)
        {
            return Divide(flow, TypeConversion.ToFloat(dividend), divisor);
        }

        public static ScalarValue Divide(FlowController flow, double dividend, int divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloat(divisor));
        }

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

        public static Value Arithmetic(FlowController flow, Operations operation,
            int leftOperand, IntervalValue<double> rightOperand)
        {
            return Arithmetic(flow, operation, TypeConversion.ToFloat(leftOperand), rightOperand);
        }

        public static Value Arithmetic(FlowController flow, Operations operation,
            double leftOperand, IntervalValue<int> rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand,
                TypeConversion.ToFloatInterval(flow.OutSet, rightOperand));
        }

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

        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, int leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, double leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, int leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, double leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        #region Addition

        public static Value Add(FlowOutputSet outset, int augend, IntervalValue<int> addend)
        {
            // Result of addition can overflow or underflow
            if ((augend >= 0) ? (addend.End <= int.MaxValue - augend)
                : (addend.Start >= int.MinValue - augend))
            {
                return outset.CreateIntegerInterval(augend + addend.Start, augend + addend.End);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Add(outset, TypeConversion.ToFloat(augend),
                    TypeConversion.ToFloatInterval(outset, addend));
            }
        }

        public static IntervalValue<double> Add(FlowOutputSet outset, int augend,
            IntervalValue<double> addend)
        {
            return Add(outset, TypeConversion.ToFloat(augend), addend);
        }

        public static IntervalValue<double> Add(FlowOutputSet outset, double augend,
            IntervalValue<int> addend)
        {
            return Add(outset, augend, TypeConversion.ToFloatInterval(outset, addend));
        }

        public static IntervalValue<double> Add(FlowOutputSet outset, double augend,
            IntervalValue<double> addend)
        {
            return outset.CreateFloatInterval(augend + addend.Start, augend + addend.End);
        }

        #endregion Addition

        #region Subtraction

        public static Value Subtract(FlowOutputSet outset, int minuend, IntervalValue<int> subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((minuend >= 0) ? (subtrahend.Start >= minuend - int.MaxValue)
                : (subtrahend.End <= minuend - int.MinValue))
            {
                return outset.CreateIntegerInterval(minuend - subtrahend.End, minuend - subtrahend.Start);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Subtract(outset, TypeConversion.ToFloat(minuend),
                    TypeConversion.ToFloatInterval(outset, subtrahend));
            }
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset,
            int minuend, IntervalValue<double> subtrahend)
        {
            return Subtract(outset, TypeConversion.ToFloat(minuend), subtrahend);
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset, double minuend,
            IntervalValue<int> subtrahend)
        {
            return Subtract(outset, minuend, TypeConversion.ToFloatInterval(outset, subtrahend));
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset, double minuend,
            IntervalValue<double> subtrahend)
        {
            return outset.CreateFloatInterval(minuend - subtrahend.End, minuend - subtrahend.Start);
        }

        #endregion Subtraction

        #region Multiplication

        public static Value Multiply(FlowOutputSet outset, int multiplicand, IntervalValue<int> multiplier)
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
                    return outset.CreateIntegerInterval(multiplicand * multiplier.Start,
                        multiplicand * multiplier.End);
                }
                else
                {
                    return outset.CreateIntegerInterval(multiplicand * multiplier.End,
                        multiplicand * multiplier.Start);
                }
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Multiply(outset, TypeConversion.ToFloat(multiplicand),
                    TypeConversion.ToFloatInterval(outset, multiplier));
            }
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset, int multiplicand,
            IntervalValue<double> multiplier)
        {
            return Multiply(outset, TypeConversion.ToFloat(multiplicand), multiplier);
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset, double multiplicand,
            IntervalValue<int> multiplier)
        {
            return Multiply(outset, multiplicand, TypeConversion.ToFloatInterval(outset, multiplier));
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset, double multiplicand,
            IntervalValue<double> multiplier)
        {
            // When multiplicand is negative, interval is reversed and endpoints swap
            if (multiplicand >= 0.0)
            {
                return outset.CreateFloatInterval(multiplicand * multiplier.Start,
                    multiplicand * multiplier.End);
            }
            else
            {
                return outset.CreateFloatInterval(multiplicand * multiplier.End,
                    multiplicand * multiplier.Start);
            }
        }

        #endregion Multiplication

        #region Division

        public static Value Divide(FlowController flow, int dividend, IntervalValue<int> divisor)
        {
            // Not divisible numbers result to floating-point number.
            // Unfortunately, except for trivial cases, the result after division
            // is always an interval mixed of integers and floating-point numbers.
            return Divide(flow, TypeConversion.ToFloat(dividend),
                TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

        public static Value Divide(FlowController flow, int dividend,
            IntervalValue<double> divisor)
        {
            return Divide(flow, TypeConversion.ToFloat(dividend), divisor);
        }

        public static Value Divide(FlowController flow, double dividend,
            IntervalValue<int> divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

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

        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<int> leftOperand, double rightOperand)
        {
            return Arithmetic(flow, operation,
                TypeConversion.ToFloatInterval(flow.OutSet, leftOperand), rightOperand);
        }

        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<double> leftOperand, int rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand, TypeConversion.ToFloat(rightOperand));
        }

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

        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, int rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, double rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, int rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, double rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        #region Addition

        public static Value Add(FlowOutputSet outset, IntervalValue<int> augend, int addend)
        {
            return Add(outset, addend, augend);
        }

        public static IntervalValue<double> Add(FlowOutputSet outset,
            IntervalValue<int> augend, double addend)
        {
            return Add(outset, addend, augend);
        }

        public static IntervalValue<double> Add(FlowOutputSet outset,
            IntervalValue<double> augend, int addend)
        {
            return Add(outset, addend, augend);
        }

        public static IntervalValue<double> Add(FlowOutputSet outset,
            IntervalValue<double> augend, double addend)
        {
            return Add(outset, addend, augend);
        }

        #endregion Addition

        #region Subtraction

        public static Value Subtract(FlowOutputSet outset, IntervalValue<int> minuend, int subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((subtrahend >= 0) ? (minuend.Start >= int.MinValue + subtrahend)
                : (minuend.End <= int.MaxValue + subtrahend))
            {
                return outset.CreateIntegerInterval(minuend.Start - subtrahend, minuend.End - subtrahend);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Subtract(outset, TypeConversion.ToFloatInterval(outset, minuend),
                    TypeConversion.ToFloat(subtrahend));
            }
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset,
            IntervalValue<int> minuend, double subtrahend)
        {
            return Subtract(outset, TypeConversion.ToFloatInterval(outset, minuend), subtrahend);
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset,
            IntervalValue<double> minuend, int subtrahend)
        {
            return Subtract(outset, minuend, TypeConversion.ToFloat(subtrahend));
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset,
            IntervalValue<double> minuend, double subtrahend)
        {
            return outset.CreateFloatInterval(minuend.Start - subtrahend, minuend.End - subtrahend);
        }

        #endregion Subtraction

        #region Multiplication

        public static Value Multiply(FlowOutputSet outset, IntervalValue<int> multiplicand, int multiplier)
        {
            return Multiply(outset, multiplier, multiplicand);
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset,
            IntervalValue<int> multiplicand, double multiplier)
        {
            return Multiply(outset, multiplier, multiplicand);
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset,
            IntervalValue<double> multiplicand, int multiplier)
        {
            return Multiply(outset, multiplier, multiplicand);
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset,
            IntervalValue<double> multiplicand, double multiplier)
        {
            return Multiply(outset, multiplier, multiplicand);
        }

        #endregion Multiplication

        #region Division

        public static Value Divide(FlowController flow, IntervalValue<int> dividend, int divisor)
        {
            // Not divisible numbers result to floating-point number.
            // Unfortunately, except for trivial cases, the result after division
            // is always an interval mixed of integers and floating-point numbers.
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend),
                TypeConversion.ToFloat(divisor));
        }

        public static Value Divide(FlowController flow, IntervalValue<int> dividend, double divisor)
        {
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend), divisor);
        }

        public static Value Divide(FlowController flow, IntervalValue<double> dividend, int divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloat(divisor));
        }

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

        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<int> leftOperand, IntervalValue<double> rightOperand)
        {
            return Arithmetic(flow, operation,
                TypeConversion.ToFloatInterval(flow.OutSet, leftOperand), rightOperand);
        }

        public static Value Arithmetic(FlowController flow, Operations operation,
            IntervalValue<double> leftOperand, IntervalValue<int> rightOperand)
        {
            return Arithmetic(flow, operation, leftOperand,
                TypeConversion.ToFloatInterval(flow.OutSet, rightOperand));
        }

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

        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        public static Value RightAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, entireIntegerInterval);
        }

        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        public static Value LeftAbstractArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, rightOperand);
        }

        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        public static Value RightAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> leftOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, leftOperand, booleanInterval);
        }

        public static Value RightAbstractBooleanArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, booleanInterval);
        }

        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<int> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        public static Value LeftAbstractBooleanArithmetic(FlowController flow,
            Operations operation, IntervalValue<double> rightOperand)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, rightOperand);
        }

        public static Value LeftAbstractBooleanArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, booleanInterval, entireIntegerInterval);
        }

        public static Value AbstractIntegerArithmetic(FlowController flow, Operations operation)
        {
            InitalizeInternals(flow.OutSet);

            return Arithmetic(flow, operation, entireIntegerInterval, entireIntegerInterval);
        }

        public static AnyFloatValue AbstractFloatArithmetic(FlowOutputSet outset, Operations operation)
        {
            return IsArithmetic(operation) ? outset.AnyFloatValue : null;
        }

        #region Addition

        public static Value Add(FlowOutputSet outset, IntervalValue<int> augend,
            IntervalValue<int> addend)
        {
            // Result of addition can overflow or underflow
            if ((augend.Start >= 0) ? (addend.End <= int.MaxValue - augend.End)
                : ((addend.Start >= int.MinValue - augend.Start)
                && ((augend.End < 0) || (addend.End <= int.MaxValue - augend.End))))
            {
                return outset.CreateIntegerInterval(augend.Start + addend.Start, augend.Start + addend.End);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Add(outset, TypeConversion.ToFloatInterval(outset, augend),
                    TypeConversion.ToFloatInterval(outset, addend));
            }
        }

        public static IntervalValue<double> Add(FlowOutputSet outset, IntervalValue<int> augend,
            IntervalValue<double> addend)
        {
            return Add(outset, TypeConversion.ToFloatInterval(outset, augend), addend);
        }

        public static IntervalValue<double> Add(FlowOutputSet outset, IntervalValue<double> augend,
            IntervalValue<int> addend)
        {
            return Add(outset, augend, TypeConversion.ToFloatInterval(outset, addend));
        }

        public static IntervalValue<double> Add(FlowOutputSet outset, IntervalValue<double> augend,
            IntervalValue<double> addend)
        {
            return outset.CreateFloatInterval(augend.Start + addend.Start, augend.End + addend.End);
        }

        public static IntervalValue<double> AbstractIntegerAdd(FlowOutputSet outset)
        {
            return outset.CreateFloatInterval(int.MinValue * 2.0, int.MaxValue * 2.0);
        }

        #endregion Addition

        #region Subtraction

        public static Value Subtract(FlowOutputSet outset, IntervalValue<int> minuend,
            IntervalValue<int> subtrahend)
        {
            // Result of subtraction can underflow or underflow
            if ((minuend.Start >= 0) ? (subtrahend.Start >= minuend.End - int.MaxValue)
                : ((subtrahend.End <= minuend.Start - int.MinValue)
                && ((minuend.End < 0) || (subtrahend.Start >= minuend.End - int.MaxValue))))
            {
                return outset.CreateIntegerInterval(minuend.Start - subtrahend.End,
                    minuend.End - subtrahend.Start);
            }
            else
            {
                // If aritmetic overflows or underflows, result is floating-point number
                return Subtract(outset, TypeConversion.ToFloatInterval(outset, minuend),
                    TypeConversion.ToFloatInterval(outset, subtrahend));
            }
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset, IntervalValue<int> minuend,
            IntervalValue<double> subtrahend)
        {
            return Subtract(outset, TypeConversion.ToFloatInterval(outset, minuend), subtrahend);
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset, IntervalValue<double> minuend,
            IntervalValue<int> subtrahend)
        {
            return Subtract(outset, minuend, TypeConversion.ToFloatInterval(outset, subtrahend));
        }

        public static IntervalValue<double> Subtract(FlowOutputSet outset, IntervalValue<double> minuend,
            IntervalValue<double> subtrahend)
        {
            return outset.CreateFloatInterval(minuend.Start - subtrahend.End,
                minuend.End - subtrahend.Start);
        }

        #endregion Subtraction

        #region Multiplication

        public static Value Multiply(FlowOutputSet outset, IntervalValue<int> multiplicand,
            IntervalValue<int> multiplier)
        {
            // TODO: Calculate more precise result
            return outset.AnyValue;
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset, IntervalValue<int> multiplicand,
            IntervalValue<double> multiplier)
        {
            return Multiply(outset, TypeConversion.ToFloatInterval(outset, multiplicand), multiplier);
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset, IntervalValue<double> multiplicand,
            IntervalValue<int> multiplier)
        {
            return Multiply(outset, multiplicand, TypeConversion.ToFloatInterval(outset, multiplier));
        }

        public static IntervalValue<double> Multiply(FlowOutputSet outset, IntervalValue<double> multiplicand,
            IntervalValue<double> multiplier)
        {
            // TODO: Calculate more precise result
            return outset.CreateFloatInterval(double.MinValue, double.MaxValue);
        }

        #endregion Multiplication

        #region Division

        public static Value Divide(FlowController flow, IntervalValue<int> dividend,
            IntervalValue<int> divisor)
        {
            // TODO: Calculate more precise result
            return flow.OutSet.AnyValue;
        }

        public static Value Divide(FlowController flow, IntervalValue<int> dividend,
            IntervalValue<double> divisor)
        {
            return Divide(flow, TypeConversion.ToFloatInterval(flow.OutSet, dividend), divisor);
        }

        public static Value Divide(FlowController flow, IntervalValue<double> dividend,
            IntervalValue<int> divisor)
        {
            return Divide(flow, dividend, TypeConversion.ToFloatInterval(flow.OutSet, divisor));
        }

        public static Value Divide(FlowController flow, IntervalValue<double> dividend,
            IntervalValue<double> divisor)
        {
            // TODO: Calculate more precise result
            return flow.OutSet.AnyValue;
        }

        #endregion Division

        #endregion Abstract aritmetic

        #region Helper methods

        private static void InitalizeInternals(FlowOutputSet outset)
        {
            if (entireIntegerInterval == null)
            {
                entireIntegerInterval = TypeConversion.AnyIntegerToIntegerInterval(outset);
                Debug.Assert(booleanInterval == null, "All private fields are initialized together");
                booleanInterval = TypeConversion.AnyBooleanToIntegerInterval(outset);
            }
        }

        private static BooleanValue WarnDivideByZero(FlowController flow)
        {
            SetWarning(flow, "Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division by zero returns false boolean value
            // Division by floating-point zero does not return NaN or infinite, but false boolean value too
            return flow.OutSet.CreateBool(false);
        }

        private static AnyValue WarnPossibleDivideByZero(FlowController flow)
        {
            // As right operant can be range of values, can possibly be 0 too
            // That causes division by zero and returns false
            SetWarning(flow, "Division by any number, possible division by zero",
                AnalysisWarningCause.DIVISION_BY_ZERO);
            return flow.OutSet.AnyValue;
        }

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation</param>
        /// <param name="message">Message of the warning</param>
        /// <param name="cause">Cause of the warning</param>
        private static void SetWarning(FlowController flow, string message, AnalysisWarningCause cause)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial, cause);
            AnalysisWarningHandler.SetWarning(flow.OutSet, warning);
        }

        #endregion Helper methods
    }
}
