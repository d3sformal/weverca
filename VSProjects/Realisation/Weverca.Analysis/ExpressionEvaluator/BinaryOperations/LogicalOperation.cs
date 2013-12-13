using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    public static class LogicalOperation
    {
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

        public static Value Logical<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> leftOperand, bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Logical(outset, operation, rightOperand, leftOperand);
        }

        public static Value Logical<T>(FlowOutputSet outset, Operations operation,
            IntervalValue<T> leftOperand, IntervalValue<T> rightOperand)
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

        #region And

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

        public static Value And<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return And(outset, rightOperand, leftOperand);
        }

        public static Value And<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(leftOperand, out convertedValue))
            {
                return And(outset, convertedValue, rightOperand);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        #endregion And

        #region Or

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

        public static Value Or<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Or(outset, rightOperand, leftOperand);
        }

        public static Value Or<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(leftOperand, out convertedValue))
            {
                return Or(outset, convertedValue, rightOperand);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        #endregion Or

        #region Xor

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

        public static Value Xor<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            bool rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return Xor(outset, rightOperand, leftOperand);
        }

        public static Value Xor<T>(FlowOutputSet outset, IntervalValue<T> leftOperand,
            IntervalValue<T> rightOperand)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool convertedValue;

            if (TypeConversion.TryConvertToBoolean<T>(leftOperand, out convertedValue))
            {
                return Xor(outset, convertedValue, rightOperand);
            }
            else
            {
                return outset.AnyBooleanValue;
            }
        }

        #endregion Xor
    }
}
