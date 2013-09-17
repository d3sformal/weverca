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
                    if (!ComparisonOperation(leftOperand.Value, value.Value))
                    {
                        if (!LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                            TypeConversion.ToBoolean(value.Value)))
                        {
                            bool isBitwise;
                            if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                            {
                                isBitwise = BitwiseOperation(leftOperand.Value, rightInteger);
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
                    if ((leftOperand.Value == int.MinValue) && value.Value)
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
                    if (!ComparisonOperation(leftBoolean, value.Value))
                    {
                        if (!LogicalOperation(leftBoolean, value.Value))
                        {
                            if (!BitwiseOperation(leftOperand.Value, TypeConversion.ToInteger(value.Value)))
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
                case Operations.Equal:
                case Operations.Identical:
                    Result = OutSet.CreateBool(leftOperand.Value == value.Value);
                    break;
                case Operations.NotEqual:
                case Operations.NotIdentical:
                    Result = OutSet.CreateBool(leftOperand.Value != value.Value);
                    break;
                case Operations.LessThan:
                    Result = OutSet.CreateBool(leftOperand.Value < value.Value);
                    break;
                case Operations.LessThanOrEqual:
                    Result = OutSet.CreateBool(leftOperand.Value <= value.Value);
                    break;
                case Operations.GreaterThan:
                    Result = OutSet.CreateBool(leftOperand.Value > value.Value);
                    break;
                case Operations.GreaterThanOrEqual:
                    Result = OutSet.CreateBool(leftOperand.Value >= value.Value);
                    break;
                case Operations.Add:
                    // Result of addition can overflow or underflow
                    if (((value.Value >= 0) && (leftOperand.Value <= int.MaxValue - value.Value))
                        || ((value.Value < 0) && (leftOperand.Value >= int.MinValue - value.Value)))
                    {
                        Result = OutSet.CreateInt(leftOperand.Value + value.Value);
                    }
                    else
                    {
                        // If aritmetic overflows or underflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            + value.Value);
                    }
                    break;
                case Operations.Sub:
                    // Result of addition can underflow or underflow
                    if (((value.Value >= 0) && (leftOperand.Value >= int.MinValue + value.Value))
                        || ((value.Value < 0) && (leftOperand.Value <= int.MaxValue + value.Value)))
                    {
                        Result = OutSet.CreateInt(leftOperand.Value - value.Value);
                    }
                    else
                    {
                        // If aritmetic overflows or underflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value)
                            - value.Value);
                    }
                    break;
                case Operations.Mul:
                    // Result of addition can overflow or underflow
                    // TODO: Find more cleaner solution ((a * b <= c) <==> (a <= c / b))
                    var product = System.Convert.ToInt64(leftOperand.Value) * value.Value;
                    if ((product >= int.MinValue) && (product <= int.MaxValue))
                    {
                        Result = OutSet.CreateInt(System.Convert.ToInt32(product));
                    }
                    else
                    {
                        // If aritmetic overflows or underflows, result is double
                        Result = OutSet.CreateDouble(TypeConversion.ToFloat(product));
                    }
                    break;
                case Operations.Div:
                    if (value.Value != 0)
                    {
                        if ((leftOperand.Value % value.Value) == 0)
                        {
                            Result = OutSet.CreateInt(leftOperand.Value / value.Value);
                        }
                        else
                        {
                            Result = OutSet.CreateDouble(TypeConversion.ToFloat(leftOperand.Value) / value.Value);
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
                    if (!LogicalOperation(TypeConversion.ToBoolean(leftOperand.Value),
                        TypeConversion.ToBoolean(value.Value)))
                    {
                        if (!BitwiseOperation(leftOperand.Value, value.Value))
                        {
                            base.VisitIntegerValue(value);
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
