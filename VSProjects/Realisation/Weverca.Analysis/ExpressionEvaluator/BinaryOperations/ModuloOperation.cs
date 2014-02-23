using System;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// The class contains methods performing modulo operation.
    /// </summary>
    /// <remarks>
    /// When PHP performs a modulo operation, it always converts both operands into integers. Thou there
    /// are just two functions that actually perform the modulo operation, the rest of functions try
    /// to convert operands into integers. These two functions differs in type of divisor: It can be either
    /// integer or interval of integers. Modulo by integer is simple. If dividend is interval, the result
    /// is interval smaller than (0;+-dividend) interval, depending on sign of dividend. Modulo by interval
    /// is more complicated operation implemented mainly in
    /// <see cref="Modulo(FlowController, int, IntervalValue{int})" /> method. For abstract operands, it is
    /// very complicated to compute more precise result. In all cases, we must take into consideration that
    /// division (or modulo) by zero is not error and when divisor is (possibly) zero, the modulo operation
    /// is still valid expression that is evaluated to <c>false</c> value.
    /// </remarks>
    public static class ModuloOperation
    {
        private delegate Value IntegerDivisorModulo<T>(FlowController flow, T dividend, int divisor);

        private delegate Value IntervalDivisorModulo<T>(FlowController flow, T dividend,
            IntervalValue<int> divisor);

        /// <summary>
        /// Interval of all values converted from a boolean value.
        /// </summary>
        private static IntervalValue<int> booleanInterval;

        #region Integer divisor

        /// <summary>
        /// Perform modulo operation of integer dividend and divisor. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of modulo operation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static ScalarValue Modulo(FlowController flow, int dividend, int divisor)
        {
            if (divisor != 0)
            {
                // Value has the same sign as dividend
                return flow.OutSet.CreateInt(dividend % divisor);
            }
            else
            {
                return WarnModuloByZero(flow);
            }
        }

        /// <summary>
        /// Try to convert dividend into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point dividend of modulo operation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, double dividend, int divisor)
        {
            if (divisor != 0)
            {
                int convertedValue;

                // Here we distinguish whether the integer dividend is known or not
                if (TypeConversion.TryConvertToInteger(dividend, out convertedValue))
                {
                    // Value has the same sign as dividend
                    return flow.OutSet.CreateInt(convertedValue % divisor);
                }
                else
                {
                    return WorstModuloResult(flow.OutSet, divisor);
                }
            }
            else
            {
                return WarnModuloByZero(flow);
            }
        }

        /// <summary>
        /// Try to convert dividend into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">String dividend of modulo operation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, string dividend, int divisor)
        {
            if (divisor != 0)
            {
                int integerValue;
                double floatValue;
                bool isInteger;
                TypeConversion.TryConvertToNumber(dividend, false, out integerValue,
                    out floatValue, out isInteger);

                // Here we distinguish whether the integer dividend is known or not
                if (isInteger)
                {
                    // Value has the same sign as dividend
                    return flow.OutSet.CreateInt(integerValue % divisor);
                }
                else
                {
                    return WorstModuloResult(flow.OutSet, divisor);
                }
            }
            else
            {
                return WarnModuloByZero(flow);
            }
        }

        /// <summary>
        /// Perform modulo operation of interval dividend and integer divisor. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of modulo operation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<int> dividend, int divisor)
        {
            if (divisor != 0)
            {
                return Modulo(flow.OutSet, dividend, divisor);
            }
            else
            {
                return WarnModuloByZero(flow);
            }
        }

        /// <summary>
        /// Try to convert dividend into interval and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of modulo operation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<double> dividend, int divisor)
        {
            if (divisor != 0)
            {
                IntervalValue<int> convertedValue;

                // Here we distinguish whether the integer interval dividend is known or not
                if (TypeConversion.TryConvertToIntegerInterval(flow.OutSet, dividend, out convertedValue))
                {
                    return Modulo(flow.OutSet, convertedValue, divisor);
                }
                else
                {
                    return WorstModuloResult(flow.OutSet, divisor);
                }
            }
            else
            {
                return WarnModuloByZero(flow);
            }
        }

        /// <summary>
        /// Perform modulo operation of abstract boolean dividend and integer divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value LeftAbstractBooleanModulo(FlowController flow, int divisor)
        {
            InitalizeInternals(flow.OutSet);

            return Modulo(flow, booleanInterval, divisor);
        }

        /// <summary>
        /// Specify possible result of modulo operation when dividend is unknown. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Integer divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value AbstractModulo(FlowController flow, int divisor)
        {
            if (divisor != 0)
            {
                return WorstModuloResult(flow.OutSet, divisor);
            }
            else
            {
                return WarnModuloByZero(flow);
            }
        }

        #endregion Integer divisor

        #region Float divisor

        /// <summary>
        /// Try to convert divisor into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, int dividend, double divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend and divisor into integer and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, double dividend, double divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend and divisor into integer and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">String dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, string dividend, double divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert divisor into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<int> dividend, double divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend into interval and divisor to integer and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<double> dividend, double divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Perform modulo operation of abstract boolean dividend and floating-point number divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Floating-point number divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value LeftAbstractBooleanModulo(FlowController flow, double divisor)
        {
            InitalizeInternals(flow.OutSet);

            return Modulo(flow, booleanInterval, divisor);
        }

        /// <summary>
        /// Specify possible result of modulo operation when dividend is unknown. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Floating-point divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value AbstractModulo(FlowController flow, double divisor)
        {
            int convertedValue;

            // Here we distinguish whether the integer divisor is known or not
            if (TypeConversion.TryConvertToInteger(divisor, out convertedValue))
            {
                return AbstractModulo(flow, convertedValue);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        #endregion Float divisor

        #region String divisor

        /// <summary>
        /// Try to convert divisor into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of modulo operation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, int dividend, string divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend and divisor into integer and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point dividend of modulo operation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, double dividend, string divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend and divisor into integer and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">String dividend of modulo operation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, string dividend, string divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert divisor into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of modulo operation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<int> dividend, string divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend into interval and divisor to integer and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of modulo operation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<double> dividend, string divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Perform modulo operation of abstract boolean dividend and string divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value LeftAbstractBooleanModulo(FlowController flow, string divisor)
        {
            InitalizeInternals(flow.OutSet);

            return Modulo(flow, booleanInterval, divisor);
        }

        /// <summary>
        /// Specify possible result of modulo operation when dividend is unknown. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">String divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value AbstractModulo(FlowController flow, string divisor)
        {
            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(divisor, false, out integerValue,
                out floatValue, out isInteger);

            // Here we distinguish whether the integer divisor is known or not
            if (isInteger)
            {
                return AbstractModulo(flow, integerValue);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        #endregion String divisor

        #region Integer interval divisor

        /// <summary>
        /// Perform modulo operation of integer dividend and interval divisor. Warn if modulo by zero.
        /// </summary>
        /// <remarks>
        /// Modulo by interval can give very nice result. The result must be inside interval. If dividend is
        /// inside interval, the resulting interval is limited by them. And finally if positive respectively
        /// negative interval divisor is greater than positive respectively less than negative dividend,
        /// the result is dividend itself.
        /// </remarks>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of modulo operation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, int dividend, IntervalValue<int> divisor)
        {
            if (dividend > int.MinValue)
            {
                int leftBound;
                int rightBound;

                // We make maximal bounds of result interval
                if (dividend >= 0)
                {
                    leftBound = -dividend;
                    rightBound = dividend;
                }
                else
                {
                    leftBound = dividend;
                    rightBound = -dividend;
                }

                if ((divisor.Start <= rightBound) && (divisor.End >= leftBound))
                {
                    if ((divisor.Start > 0) || (divisor.End < 0))
                    {
                        // Modulo of zero is always zero
                        if (dividend == 0)
                        {
                            return flow.OutSet.CreateInt(0);
                        }

                        // The integer interval divisor is either positive or negative
                        if ((divisor.Start >= leftBound) && (divisor.End <= rightBound))
                        {
                            // We can better aproximate the interval if result of division is never zero
                            var halfDividend = ((leftBound - 1) / 2) + 1;
                            int maxBound;

                            if (divisor.Start > 0)
                            {
                                maxBound = Math.Max(halfDividend, 1 - divisor.End);
                            }
                            else
                            {
                                maxBound = Math.Max(halfDividend, divisor.Start + 1);
                            }

                            if (dividend >= 0)
                            {
                                return flow.OutSet.CreateIntegerInterval(0, -maxBound);
                            }
                            else
                            {
                                return flow.OutSet.CreateIntegerInterval(maxBound, 0);
                            }
                        }
                        else
                        {
                            if (dividend >= 0)
                            {
                                return flow.OutSet.CreateIntegerInterval(0, rightBound);
                            }
                            else
                            {
                                return flow.OutSet.CreateIntegerInterval(leftBound, 0);
                            }
                        }
                    }
                    else
                    {
                        return WarnPossibleModuloByZero(flow);
                    }
                }
                else
                {
                    // Result of division is always zero, so the remainder is always the same
                    return flow.OutSet.CreateInt(dividend);
                }
            }
            else
            {
                if ((divisor.Start > 0) || (divisor.End < 0))
                {
                    var halfDivident = (dividend / 2) + 1;
                    var intervalBound = (divisor.Start > 0) ? -divisor.End : divisor.Start;
                    var maxBound = Math.Max(halfDivident, intervalBound + 1);
                    return flow.OutSet.CreateIntegerInterval(maxBound, 0);
                }
                else
                {
                    return WarnPossibleModuloByZero(flow);
                }
            }
        }

        /// <summary>
        /// Try to convert dividend into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point dividend of modulo operation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, double dividend, IntervalValue<int> divisor)
        {
            int convertedValue;

            // Here we distinguish whether the integer dividend is known or not
            if (TypeConversion.TryConvertToInteger(dividend, out convertedValue))
            {
                return Modulo(flow, convertedValue, divisor);
            }
            else
            {
                return AbstractModulo(flow, divisor);
            }
        }

        /// <summary>
        /// Try to convert dividend into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">String dividend of modulo operation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, string dividend, IntervalValue<int> divisor)
        {
            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(dividend, false, out integerValue,
                out floatValue, out isInteger);

            // Here we distinguish whether the integer dividend is known or not
            if (isInteger)
            {
                return Modulo(flow, integerValue, divisor);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        /// <summary>
        /// Perform modulo operation of integer interval dividend divisor. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of modulo operation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<int> dividend,
            IntervalValue<int> divisor)
        {
            // TODO: Calculate more precise result
            return AbstractModulo(flow, divisor);
        }

        /// <summary>
        /// Try to convert dividend into interval and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of modulo operation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<double> dividend,
            IntervalValue<int> divisor)
        {
            IntervalValue<int> convertedValue;

            // Here we distinguish whether the integer interval dividend is known or not
            if (TypeConversion.TryConvertToIntegerInterval(flow.OutSet, dividend, out convertedValue))
            {
                return Modulo(flow, convertedValue, divisor);
            }
            else
            {
                return AbstractModulo(flow, divisor);
            }
        }

        /// <summary>
        /// Perform modulo operation of abstract boolean dividend and integer interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value LeftAbstractBooleanModulo(FlowController flow, IntervalValue<int> divisor)
        {
            InitalizeInternals(flow.OutSet);

            return Modulo(flow, booleanInterval, divisor);
        }

        /// <summary>
        /// Specify possible result of modulo operation when dividend is unknown. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Integer interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value AbstractModulo(FlowController flow, IntervalValue<int> divisor)
        {
            if (divisor.Start > 0)
            {
                return flow.OutSet.CreateIntegerInterval(1 - divisor.End, divisor.End - 1);
            }
            else if (divisor.End < 0)
            {
                var bound = divisor.Start + 1;
                return flow.OutSet.CreateIntegerInterval(bound, -bound);
            }
            else
            {
                if ((divisor.Start < 0) || (divisor.End > 0))
                {
                    return WarnPossibleModuloByZero(flow);
                }
                else
                {
                    return WarnModuloByZero(flow);
                }
            }
        }

        #endregion Integer interval divisor

        #region Float interval divisor

        /// <summary>
        /// Try to convert divisor into integer and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, int dividend, IntervalValue<double> divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend into integer and divisor to interval and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, double dividend, IntervalValue<double> divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend into integer and divisor to interval and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">String dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, string dividend, IntervalValue<double> divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert divisor into interval and perform modulo operation. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Integer interval dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<int> dividend,
            IntervalValue<double> divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Try to convert dividend and divisor into integer interval and perform modulo operation.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="dividend">Floating-point interval dividend of modulo operation.</param>
        /// <param name="divisor">Floating-point interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value Modulo(FlowController flow, IntervalValue<double> dividend,
            IntervalValue<double> divisor)
        {
            return Modulo(flow, Modulo, dividend, divisor);
        }

        /// <summary>
        /// Perform modulo operation of abstract boolean dividend and floating-point number interval divisor.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Floating-point number interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value LeftAbstractBooleanModulo(FlowController flow, IntervalValue<double> divisor)
        {
            InitalizeInternals(flow.OutSet);

            return Modulo(flow, booleanInterval, divisor);
        }

        /// <summary>
        /// Specify possible result of modulo operation when dividend is unknown. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Floating-point interval divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise remainder.</returns>
        public static Value AbstractModulo(FlowController flow, IntervalValue<double> divisor)
        {
            IntervalValue<int> convertedValue;
            if (TypeConversion.TryConvertToIntegerInterval(flow.OutSet, divisor, out convertedValue))
            {
                return AbstractModulo(flow, convertedValue);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        #endregion Float interval divisor

        /// <summary>
        /// Perform modulo operation with boolean divisor. Warn if modulo by zero.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <param name="divisor">Boolean divisor of modulo operation.</param>
        /// <returns><c>false</c> whether <paramref name="divisor" /> is zero, otherwise zero.</returns>
        public static ScalarValue ModuloByBooleanValue(FlowController flow, bool divisor)
        {
            if (divisor)
            {
                // Modulo by 1 (true) is always 0
                return flow.OutSet.CreateInt(0);
            }
            else
            {
                SetWarning(flow, "Modulo by zero (converted from boolean false)",
                    AnalysisWarningCause.DIVISION_BY_ZERO);

                // Division or modulo by false returns false boolean value
                return flow.OutSet.CreateBool(false);
            }
        }

        /// <summary>
        /// Warn that modulo by abstract boolean value has occurred and return any abstract value.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <returns>Any abstract value.</returns>
        public static AnyValue ModuloByAnyBooleanValue(FlowController flow)
        {
            SetWarning(flow, "Possible division by zero (converted from boolean false)",
                AnalysisWarningCause.DIVISION_BY_ZERO);

            // Division or modulo by false returns false boolean value
            return flow.OutSet.AnyValue;
        }

        /// <summary>
        /// Warn that modulo by null has occurred and return result of operation, <c>false</c> value.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <returns><c>false</c> value that is result of modulo by zero.</returns>
        public static BooleanValue ModuloByNull(FlowController flow)
        {
            SetWarning(flow, "Modulo by zero (converted from null)",
                AnalysisWarningCause.DIVISION_BY_ZERO);

            // Modulo by null returns false boolean value
            return flow.OutSet.CreateBool(false);
        }

        /// <summary>
        /// Specify possible result of modulo operation when both dividend and divisor are unknown.
        /// </summary>
        /// <param name="flow">Flow controller of program point providing data for evaluation.</param>
        /// <returns>Any abstract value.</returns>
        public static AnyValue AbstractModulo(FlowController flow)
        {
            return WarnPossibleModuloByZero(flow);
        }

        #region Helper methods

        private static void InitalizeInternals(ISnapshotReadWrite snapshot)
        {
            if (booleanInterval == null)
            {
                booleanInterval = TypeConversion.AnyBooleanToIntegerInterval(snapshot);
            }
        }

        private static Value Modulo<T>(FlowController flow, IntegerDivisorModulo<T> operation,
            T dividend, double divisor)
        {
            int convertedValue;

            // Here we distinguish whether the integer divisor is known or not
            if (TypeConversion.TryConvertToInteger(divisor, out convertedValue))
            {
                return operation(flow, dividend, convertedValue);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        private static Value Modulo<T>(FlowController flow, IntegerDivisorModulo<T> operation,
            T dividend, string divisor)
        {
            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(divisor, false, out integerValue,
                out floatValue, out isInteger);

            // Here we distinguish whether the integer divisor is known or not
            if (isInteger)
            {
                return operation(flow, dividend, integerValue);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        private static Value Modulo<T>(FlowController flow, IntervalDivisorModulo<T> operation,
            T dividend, IntervalValue<double> divisor)
        {
            IntervalValue<int> convertedValue;
            if (TypeConversion.TryConvertToIntegerInterval(flow.OutSet, divisor, out convertedValue))
            {
                return operation(flow, dividend, convertedValue);
            }
            else
            {
                return WarnPossibleModuloByZero(flow);
            }
        }

        private static Value Modulo(ISnapshotReadWrite snapshot, IntervalValue<int> leftOperand,
            int rightOperand)
        {
            IntervalValue<int> result;

            if (leftOperand.Start >= 0)
            {
                result = PositiveDividendModulo(snapshot, leftOperand.Start,
                    leftOperand.End, rightOperand);
            }
            else
            {
                if (leftOperand.End <= 0)
                {
                    result = NegativeDividendModulo(snapshot, leftOperand.Start,
                        leftOperand.End, rightOperand);
                }
                else
                {
                    var negative = NegativeDividendModulo(snapshot, leftOperand.Start, 0, rightOperand);
                    var positive = PositiveDividendModulo(snapshot, 0, leftOperand.End, rightOperand);
                    result = snapshot.CreateIntegerInterval(negative.Start, positive.End);
                }
            }

            if (result.Start < result.End)
            {
                return result;
            }
            else
            {
                return snapshot.CreateInt(result.Start);
            }
        }

        private static IntervalValue<int> PositiveDividendModulo(ISnapshotReadWrite snapshot,
            int leftOperandStart, int leftOperandEnd, int rightOperand)
        {
            if (rightOperand < 0)
            {
                if (rightOperand > int.MinValue)
                {
                    rightOperand = -rightOperand;
                }
                else
                {
                    return snapshot.CreateIntegerInterval(leftOperandStart, leftOperandEnd);
                }
            }

            if ((leftOperandEnd - leftOperandStart) >= rightOperand - 1)
            {
                return snapshot.CreateIntegerInterval(0, rightOperand - 1);
            }
            else
            {
                var resultStart = leftOperandStart % rightOperand;
                var resultEnd = leftOperandEnd % rightOperand;

                if (resultStart <= resultEnd)
                {
                    return snapshot.CreateIntegerInterval(resultStart, resultEnd);
                }
                else
                {
                    return snapshot.CreateIntegerInterval(0, rightOperand - 1);
                }
            }
        }

        private static IntervalValue<int> NegativeDividendModulo(ISnapshotReadWrite snapshot,
            int leftOperandStart, int leftOperandEnd, int rightOperand)
        {
            if (rightOperand < 0)
            {
                if (rightOperand > int.MinValue)
                {
                    rightOperand = -rightOperand;
                }
                else
                {
                    if (leftOperandStart > int.MinValue)
                    {
                        return snapshot.CreateIntegerInterval(leftOperandStart, leftOperandEnd);
                    }
                    else
                    {
                        if (leftOperandEnd > int.MinValue)
                        {
                            return snapshot.CreateIntegerInterval(int.MinValue + 1, 0);
                        }
                        else
                        {
                            return snapshot.CreateIntegerInterval(0, 0);
                        }
                    }
                }
            }

            if ((leftOperandEnd - leftOperandStart) >= rightOperand - 1)
            {
                return snapshot.CreateIntegerInterval(1 - rightOperand, 0);
            }
            else
            {
                var resultStart = leftOperandStart % rightOperand;
                var resultEnd = leftOperandEnd % rightOperand;

                if (resultStart <= resultEnd)
                {
                    return snapshot.CreateIntegerInterval(resultStart, resultEnd);
                }
                else
                {
                    return snapshot.CreateIntegerInterval(1 - rightOperand, 0);
                }
            }
        }

        private static IntervalValue<int> WorstModuloResult(ISnapshotReadWrite snapshot, int divisor)
        {
            Debug.Assert(divisor != 0, "Zero divisor causes modulo by zero");

            if (divisor > 0)
            {
                return snapshot.CreateIntegerInterval(1 - divisor, divisor - 1);
            }
            else
            {
                int bound = divisor + 1;
                return snapshot.CreateIntegerInterval(bound, -bound);
            }
        }

        private static BooleanValue WarnModuloByZero(FlowController flow)
        {
            SetWarning(flow, "Modulo by zero", AnalysisWarningCause.DIVISION_BY_ZERO);

            // Modulo by zero returns false boolean value
            return flow.OutSet.CreateBool(false);
        }

        private static AnyValue WarnPossibleModuloByZero(FlowController flow)
        {
            // As right operant can be range of values, can possibly be 0 too
            // That causes division by zero and returns false
            SetWarning(flow, "Division by any integer, possible division by zero",
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
            var warning = new AnalysisWarning(flow.CurrentScript.FullName,
                message, flow.CurrentPartial, cause);
            AnalysisWarningHandler.SetWarning(flow.OutSet, warning);
        }

        #endregion Helper methods
    }
}
