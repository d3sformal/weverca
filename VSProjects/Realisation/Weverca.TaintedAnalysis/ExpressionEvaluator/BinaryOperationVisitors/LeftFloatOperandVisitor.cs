using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal class LeftFloatOperandVisitor : LeftOperandVisitor
    {
        private FloatValue leftOperand;

        internal LeftFloatOperandVisitor(FloatValue value, ExpressionEvaluator expressionEvaluator)
            : base(expressionEvaluator)
        {
            leftOperand = value;
        }

        #region IValueVisitor Members

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (Operation)
            {
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.Or:
                case Operations.Xor:
                    Result = TypeConversion.ToBoolean(OutSet, leftOperand);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    Result = OutSet.CreateBool(!TypeConversion.ToBoolean(leftOperand.Value));
                    break;
                case Operations.Identical:
                case Operations.LessThan:
                case Operations.And:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.GreaterThanOrEqual:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Concat:
                    Result = TypeConversion.ToString(OutSet, leftOperand);
                    break;
                case Operations.Add:
                case Operations.Sub:
                    Result = OutSet.CreateDouble(leftOperand.Value);
                    break;
                case Operations.Mul:
                    Result = OutSet.CreateDouble((leftOperand.Value >= 0.0) ? 0.0 : -0.0);
                    break;
                case Operations.BitAnd:
                    Result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    int leftInteger;
                    if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                    {
                        Result = OutSet.CreateInt(leftInteger);
                    }
                    else
                    {
                        Result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Div:
                case Operations.Mod:
                    evaluator.SetWarning("Division by zero (converted from null)",
                        AnalysisWarningCause.DIVISION_BY_ZERO);
                    // Division by null returns false boolean value
                    Result = OutSet.CreateBool(false);
                    break;
                default:
                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        public override void VisitFloatValue(FloatValue value)
        {
            int leftInteger, rightInteger;

            switch (Operation)
            {
                case Operations.Identical:
                    evaluator.SetWarning("Comparing floating-point numbers directly for equality");
                    Result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    evaluator.SetWarning("Comparing floating-point numbers directly for non-equality");
                    Result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                case Operations.Add:
                    Result = OutSet.CreateDouble(leftOperand.Value + value.Value);
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateDouble(leftOperand.Value - value.Value);
                    break;
                case Operations.Mul:
                    Result = OutSet.CreateDouble(leftOperand.Value * value.Value);
                    break;
                case Operations.Div:
                    if (value.Value != 0.0)
                    {
                        Result = OutSet.CreateDouble(leftOperand.Value / value.Value);
                    }
                    else
                    {
                        evaluator.SetWarning("Division by floating-point zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by floating-point zero does not return NaN, but false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Mod:
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (rightInteger != 0)
                        {
                            if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                            {
                                // Value has the same sign as dividend
                                Result = OutSet.CreateInt(leftInteger % rightInteger);
                            }
                            else
                            {
                                Result = OutSet.AnyIntegerValue;
                            }
                        }
                        else
                        {
                            evaluator.SetWarning("Division by floating-point zero",
                                AnalysisWarningCause.DIVISION_BY_ZERO);
                            // Division by floating-point zero does not return NaN, but false boolean value
                            Result = OutSet.CreateBool(false);
                        }
                    }
                    else
                    {
                        // As right operant can has any value, can be 0 too
                        // That causes division by zero and returns false
                        evaluator.SetWarning("Division by any integer, possible division by zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        Result = OutSet.AnyValue;
                    }
                    break;
                case Operations.Concat:
                    Result = OutSet.CreateString(TypeConversion.ToString(leftOperand.Value)
                        + TypeConversion.ToString(value.Value));
                    break;
                default:
                    if (!ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        if (!LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                            TypeConversion.ToBoolean(value.Value)))
                        {
                            bool isBitwise;
                            if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger)
                                && TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                            {
                                isBitwise = BitwiseOperation(leftInteger, rightInteger);
                            }
                            else
                            {
                                isBitwise = IsOperationBitwise();
                                if (isBitwise)
                                {
                                    Result = OutSet.AnyIntegerValue;
                                }
                            }
                            if (!isBitwise)
                            {
                                base.VisitFloatValue(value);
                            }
                        }
                    }
                    break;
            }
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            int leftInteger;

            switch (Operation)
            {
                case Operations.Identical:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Add:
                    Result = OutSet.CreateDouble(leftOperand.Value + TypeConversion.ToFloat(value.Value));
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateDouble(leftOperand.Value - TypeConversion.ToFloat(value.Value));
                    break;
                case Operations.Mul:
                    if (value.Value)
                    {
                        Result = OutSet.CreateDouble(leftOperand.Value);
                    }
                    else
                    {
                        Result = OutSet.CreateDouble(0.0);
                    }
                    break;
                case Operations.Div:
                    if (value.Value)
                    {
                        Result = OutSet.CreateDouble(leftOperand.Value);
                    }
                    else
                    {
                        evaluator.SetWarning("Division by zero (converted from boolean false)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by false returns false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Mod:
                    if (value.Value)
                    {
                        if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                        {
                            Result = OutSet.CreateInt(0);
                        }
                        else
                        {
                            Result = OutSet.AnyIntegerValue;
                        }
                    }
                    else
                    {
                        evaluator.SetWarning("Division by zero (converted from boolean false)",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by false returns false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Concat:
                    Result = OutSet.CreateString(TypeConversion.ToString(leftOperand.Value)
                        + TypeConversion.ToString(value.Value));
                    break;
                default:
                    var leftBoolean = TypeConversion.ToBoolean(leftOperand.Value);
                    if (!ComparisonOperation(leftBoolean, value.Value))
                    {
                        if (!LogicalOperation(leftBoolean, value.Value))
                        {
                            bool isBitwise;
                            if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                            {
                                isBitwise = BitwiseOperation(leftInteger, TypeConversion.ToInteger(value.Value));
                            }
                            else
                            {
                                isBitwise = IsOperationBitwise();
                                if (isBitwise)
                                {
                                    Result = OutSet.AnyIntegerValue;
                                }
                            }
                            if (!isBitwise)
                            {
                                base.VisitBooleanValue(value);
                            }
                        }
                    }
                    break;
            }
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            int leftInteger;

            switch (Operation)
            {
                case Operations.Identical:
                    evaluator.SetWarning("Comparing floating-point numbers directly for equality");
                    Result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    evaluator.SetWarning("Comparing floating-point numbers directly for non-equality");
                    Result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                case Operations.Add:
                    Result = OutSet.CreateDouble(leftOperand.Value + value.Value);
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateDouble(leftOperand.Value - value.Value);
                    break;
                case Operations.Mul:
                    Result = OutSet.CreateDouble(leftOperand.Value * value.Value);
                    break;
                case Operations.Div:
                    if (value.Value != 0)
                    {
                        Result = OutSet.CreateDouble(leftOperand.Value / value.Value);
                    }
                    else
                    {
                        evaluator.SetWarning("Division by floating-point zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by floating-point zero does not return NaN, but false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Mod:
                    if (value.Value != 0)
                    {
                        if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                        {
                            // Value has the same sign as dividend
                            Result = OutSet.CreateInt(leftInteger % value.Value);
                        }
                        else
                        {
                            Result = OutSet.AnyIntegerValue;
                        }
                    }
                    else
                    {
                        evaluator.SetWarning("Division by floating-point zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by floating-point zero does not return NaN, but false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Concat:
                    Result = OutSet.CreateString(TypeConversion.ToString(leftOperand.Value)
                        + TypeConversion.ToString(value.Value));
                    break;
                default:
                    if (!ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        if (!LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                            TypeConversion.ToBoolean(value.Value)))
                        {
                            bool isBitwise;
                            if (TypeConversion.TryConvertToInteger(leftOperand.Value, out leftInteger))
                            {
                                isBitwise = BitwiseOperation(leftInteger, value.Value);
                            }
                            else
                            {
                                isBitwise = IsOperationBitwise();
                                if (isBitwise)
                                {
                                    Result = OutSet.AnyIntegerValue;
                                }
                            }
                            if (!isBitwise)
                            {
                                base.VisitIntegerValue(value);
                            }
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
