using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal class LeftNullOperandVisitor : LeftOperandVisitor
    {
        private UndefinedValue leftOperand;

        internal LeftNullOperandVisitor(UndefinedValue value, ExpressionEvaluator expressionEvaluator)
            : base(expressionEvaluator)
        {
            leftOperand = value;
        }

        #region IValueVisitor Members

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (Operation)
            {
                case Operations.Equal:
                case Operations.Identical:
                case Operations.LessThanOrEqual:
                case Operations.GreaterThanOrEqual:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.NotEqual:
                case Operations.NotIdentical:
                case Operations.LessThan:
                case Operations.GreaterThan:
                case Operations.And:
                case Operations.Or:
                case Operations.Xor:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.Concat:
                    Result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Add:
                case Operations.Sub:
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    Result = OutSet.CreateInt(0);
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
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    Result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    Result = OutSet.CreateBool(!TypeConversion.ToBoolean(value.Value));
                    break;
                case Operations.Identical:
                case Operations.GreaterThan:
                case Operations.And:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.LessThanOrEqual:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Concat:
                    Result = TypeConversion.ToString(OutSet, leftOperand);
                    break;
                case Operations.Add:
                    Result = OutSet.CreateDouble(value.Value);
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateDouble(-value.Value);
                    break;
                case Operations.Mul:
                    Result = OutSet.CreateDouble((value.Value >= 0.0) ? 0.0 : -0.0);
                    break;
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    Result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                    if (TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        Result = OutSet.CreateInt(rightInteger);
                    }
                    else
                    {
                        Result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Div:
                    if (value.Value != 0.0)
                    {
                        // 0.0 (null) divided by any float is always (+/-)0.0
                        Result = OutSet.CreateDouble((value.Value >= 0.0) ? 0.0 : -0.0);
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
                            // 0 (null) modulo by any float is always 0
                            Result = OutSet.CreateInt(0);
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
                default:
                    base.VisitFloatValue(value);
                    break;
            }
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (Operation)
            {
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    Result = OutSet.CreateBool(value.Value);
                    break;
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    Result = OutSet.CreateBool(!value.Value);
                    break;
                case Operations.Identical:
                case Operations.GreaterThan:
                case Operations.And:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.LessThanOrEqual:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Sub:
                    Result = OutSet.CreateInt(-TypeConversion.ToInteger(value.Value));
                    break;
                case Operations.Concat:
                    Result = TypeConversion.ToString(OutSet, leftOperand);
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    Result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    Result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.Div:
                case Operations.Mod:
                    if (value.Value)
                    {
                        // 0 (null) modulo or divided by 1 (true) is always 0
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
                default:
                    base.VisitBooleanValue(value);
                    break;
            }
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (Operation)
            {
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.Or:
                case Operations.Xor:
                    Result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Equal:
                case Operations.GreaterThanOrEqual:
                    Result = OutSet.CreateBool(!TypeConversion.ToBoolean(value.Value));
                    break;
                case Operations.Identical:
                case Operations.GreaterThan:
                case Operations.And:
                    Result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.LessThanOrEqual:
                    Result = OutSet.CreateBool(true);
                    break;
                case Operations.Sub:
                    // Result of subtraction can overflow
                    if ((value.Value == 0) || ((-value.Value) != 0))
                    {
                        Result = OutSet.CreateInt(-value.Value);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationVisitor.VisitIntegerValue"/>
                        Result = OutSet.CreateDouble(-(TypeConversion.ToFloat(value.Value)));
                    }
                    break;
                case Operations.Concat:
                    Result = TypeConversion.ToString(OutSet, leftOperand);
                    break;
                case Operations.Mul:
                case Operations.BitAnd:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    Result = OutSet.CreateInt(0);
                    break;
                case Operations.Add:
                case Operations.BitOr:
                case Operations.BitXor:
                    Result = OutSet.CreateInt(value.Value);
                    break;
                case Operations.Div:
                case Operations.Mod:
                    if (value.Value != 0)
                    {
                        // 0 (null) modulo or divided by any integer is always 0
                        Result = OutSet.CreateInt(0);
                    }
                    else
                    {
                        evaluator.SetWarning("Division by zero",
                            AnalysisWarningCause.DIVISION_BY_ZERO);
                        // Division by zero returns false boolean value
                        Result = OutSet.CreateBool(false);
                    }
                    break;
                default:
                    base.VisitIntegerValue(value);
                    break;
            }
        }

        #endregion
    }
}
