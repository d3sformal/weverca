using System;

using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal class LeftIntegerOperandVisitor : LeftOperandVisitor
    {
        private IntegerValue leftOperand;

        internal LeftIntegerOperandVisitor(IntegerValue value, ExpressionEvaluator expressionEvaluator)
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
                case Operations.Mul:
                case Operations.BitAnd:
                    Result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    Result = OutSet.CreateInt(leftOperand.Value);
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
            int rightInteger;

            switch (Operation)
            {
                case Operations.Identical:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (rightInteger != 0)
                        {
                            Result = OutSet.CreateInt(leftOperand.Value % rightInteger);
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
                    if (ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
                    {
                        break;
                    }

                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (BitwiseOperation(leftOperand.Value, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (IsOperationBitwise())
                        {
                            Result = OutSet.AnyIntegerValue;
                            break;
                        }
                    }

                    base.VisitFloatValue(value);
                    break;
            }
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (Operation)
            {
                case Operations.Identical:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Add:
                    // Result of addition can overflow
                    if ((leftOperand.Value >= int.MaxValue) && value.Value)
                    {
                        // If aritmetic overflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            + TypeConversion.ToFloat(value.Value));
                    }
                    else
                    {
                        Result = OutSet.CreateInt(leftOperand.Value + TypeConversion.ToInteger(value.Value));
                    }
                    break;
                case Operations.Sub:
                    // Result of addition can underflow
                    if ((leftOperand.Value <= int.MinValue) && value.Value)
                    {
                        // If aritmetic underflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            - TypeConversion.ToFloat(value.Value));
                    }
                    else
                    {
                        Result = OutSet.CreateInt(leftOperand.Value - TypeConversion.ToInteger(value.Value));
                    }
                    break;
                case Operations.Mul:
                    if (value.Value)
                    {
                        Result = OutSet.CreateInt(leftOperand.Value);
                    }
                    else
                    {
                        Result = OutSet.CreateInt(0);
                    }
                    break;
                case Operations.Div:
                    if (value.Value)
                    {
                        Result = OutSet.CreateInt(leftOperand.Value);
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
                        Result = OutSet.CreateInt(0);
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
                    if (ComparisonOperation(leftBoolean, value.Value))
                    {
                        break;
                    }

                    if (LogicalOperation(leftBoolean, value.Value))
                    {
                        break;
                    }

                    if (BitwiseOperation(leftOperand.Value, TypeConversion.ToInteger(value.Value)))
                    {
                        break;
                    }

                    base.VisitBooleanValue(value);
                    break;
            }
        }

        public override void VisitStringValue(StringValue value)
        {
            switch (Operation)
            {
                case Operations.Identical:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    // TODO: There is a problem with conversion
                    throw new NotImplementedException();
                case Operations.Concat:
                    Result = OutSet.CreateString(TypeConversion.ToString(leftOperand.Value) + value.Value);
                    break;
                default:
                    if (LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
                    {
                        break;
                    }

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(value.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (isInteger)
                    {
                        if (ComparisonOperation(leftOperand.Value, integerValue))
                        {
                            break;
                        }

                        if (ArithmeticOperation(leftOperand.Value, integerValue))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (ComparisonOperation(leftOperand.Value, floatValue))
                        {
                            break;
                        }

                        if (ArithmeticOperation(leftOperand.Value, floatValue))
                        {
                            break;
                        }
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    if (isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                    {
                        if (BitwiseOperation(leftOperand.Value, integerValue))
                        {
                            break;
                        }
                    }
                    else
                    {
                        // If at least one operand can not be recognized, result can be any integer value.
                        if (IsOperationBitwise())
                        {
                            Result = OutSet.AnyIntegerValue;
                            break;
                        }
                    }

                    base.VisitStringValue(value);
                    break;
            }
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (Operation)
            {
                case Operations.Identical:
                    Result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                case Operations.Mod:
                    if (value.Value != 0)
                    {
                        // Value has the same sign as dividend
                        Result = OutSet.CreateInt(leftOperand.Value % value.Value);
                    }
                    else
                    {
                        evaluator.SetWarning("Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by zero returns false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Concat:
                    Result = OutSet.CreateString(TypeConversion.ToString(leftOperand.Value)
                        + TypeConversion.ToString(value.Value));
                    break;
                default:
                    if (ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (ArithmeticOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    if (LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
                    {
                        break;
                    }

                    if (BitwiseOperation(leftOperand.Value, value.Value))
                    {
                        break;
                    }

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        #endregion
    }
}
