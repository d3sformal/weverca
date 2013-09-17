using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal class LeftBooleanOperandVisitor : LeftOperandVisitor
    {
        private BooleanValue leftOperand;

        internal LeftBooleanOperandVisitor(BooleanValue value, ExpressionEvaluator expressionEvaluator)
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
                    Result = OutSet.CreateBool(leftOperand.Value);
                    break;
                case Operations.Equal:
                case Operations.LessThanOrEqual:
                    Result = OutSet.CreateBool(!leftOperand.Value);
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
                    Result = TypeConversion.ToInteger(OutSet, leftOperand);
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
                case Operations.Add:
                    Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value) + value.Value);
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value) - value.Value);
                    break;
                case Operations.Mul:
                    if (leftOperand.Value)
                    {
                        Result = OutSet.CreateDouble(value.Value);
                    }
                    else
                    {
                        Result = OutSet.CreateDouble(0.0);
                    }
                    break;
                case Operations.Div:
                    if (value.Value != 0.0)
                    {
                        if (leftOperand.Value)
                        {
                            Result = OutSet.CreateDouble(1.0 / value.Value);
                        }
                        else
                        {
                            Result = OutSet.CreateDouble(0.0);
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
                case Operations.Mod:
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (rightInteger != 0)
                        {
                            Result = OutSet.CreateInt((leftOperand.Value && (rightInteger != 1)
                                && (rightInteger != -1)) ? 1 : 0);
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
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    if (!ComparisonOperation(leftOperand.Value, rightBoolean))
                    {
                        if (!LogicalOperation(leftOperand.Value, rightBoolean))
                        {
                            bool isBitwise;
                            if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                            {
                                isBitwise = BitwiseOperation(TypeConversion.ToInteger(leftOperand.Value), rightInteger);
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
            switch (Operation)
            {
                case Operations.Identical:
                    Result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                case Operations.Add:
                    Result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value)
                        + TypeConversion.ToInteger(value.Value));
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value)
                        - TypeConversion.ToInteger(value.Value));
                    break;
                case Operations.Mul:
                    Result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value && value.Value));
                    break;
                case Operations.Div:
                    if (value.Value)
                    {
                        Result = TypeConversion.ToInteger(OutSet, leftOperand);
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
                        // Modulo by 1 (true) is always 0
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
                    if (!ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        if (!LogicalOperation(leftOperand.Value, value.Value))
                        {
                            if (!BitwiseOperation(TypeConversion.ToInteger(leftOperand.Value),
                                TypeConversion.ToInteger(value.Value)))
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
                    if (leftOperand.Value && (value.Value >= int.MaxValue))
                    {
                        // If aritmetic overflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            + TypeConversion.ToFloat(value.Value));
                    }
                    else
                    {
                        Result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value) + value.Value);
                    }
                    break;
                case Operations.Sub:
                    // Result of subtraction can overflow
                    if (value.Value > (int.MinValue + TypeConversion.ToInteger(leftOperand.Value)))
                    {
                        Result = OutSet.CreateInt(TypeConversion.ToInteger(leftOperand.Value) - value.Value);
                    }
                    else
                    {
                        // If aritmetic overflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            - TypeConversion.ToFloat(value.Value));
                    }
                    break;
                case Operations.Mul:
                    if (leftOperand.Value)
                    {
                        Result = OutSet.CreateInt(value.Value);
                    }
                    else
                    {
                        Result = OutSet.CreateInt(0);
                    }
                    break;
                case Operations.Div:
                    if (value.Value != 0)
                    {
                        if ((value.Value == 1) || (value.Value == -1))
                        {
                            Result = OutSet.CreateInt(leftOperand.Value ? value.Value : 0);
                        }
                        else
                        {
                            Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                                / TypeConversion.ToFloat(value.Value));
                        }
                    }
                    else
                    {
                        evaluator.SetWarning("Division by zero", AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by zero returns false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                case Operations.Mod:
                    if (value.Value != 0)
                    {
                        Result = OutSet.CreateInt((leftOperand.Value && (value.Value != 1)
                            && (value.Value != -1)) ? 1 : 0);
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
                    var rightBoolean = TypeConversion.ToBoolean(value.Value);
                    if (!ComparisonOperation(leftOperand.Value, rightBoolean))
                    {
                        if (!LogicalOperation(leftOperand.Value, rightBoolean))
                        {
                            if (!BitwiseOperation(TypeConversion.ToInteger(leftOperand.Value), value.Value))
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
