using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    internal class LeftStringOperandVisitor : LeftOperandVisitor
    {
        private StringValue leftOperand;

        internal LeftStringOperandVisitor(StringValue value,
            ExpressionEvaluator expressionEvaluator)
            : base(expressionEvaluator)
        {
            leftOperand = value;
        }

        #region IValueVisitor Members

        public override void VisitFloatValue(FloatValue value)
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
                    Result = OutSet.CreateString(leftOperand.Value + TypeConversion.ToString(value.Value));
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
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (ComparisonOperation(floatValue, value.Value))
                    {
                        break;
                    }

                    if (ArithmeticOperation(floatValue, value.Value))
                    {
                        break;
                    }

                    // If string has hexadecimal format, the first zero is recognized.
                    if (isHexadecimal)
                    {
                        integerValue = 0;
                    }

                    int rightInteger;
                    if ((isInteger || (isSuccessful
                        && TypeConversion.TryConvertToInteger(floatValue, out integerValue)))
                        && TypeConversion.TryConvertToInteger(value.Value, out rightInteger))
                    {
                        if (BitwiseOperation(integerValue, rightInteger))
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
                case Operations.Mod:
                    // TODO: There is a problem with conversion
                    throw new NotImplementedException();
                case Operations.Concat:
                    Result = OutSet.CreateString(leftOperand.Value + TypeConversion.ToString(value.Value));
                    break;
                default:
                    var booleanValue = TypeConversion.ToBoolean(leftOperand.Value);
                    if (LogicalOperation(booleanValue, value.Value))
                    {
                        break;
                    }

                    if (ComparisonOperation(booleanValue, value.Value))
                    {
                        break;
                    }

                    var rightInteger = TypeConversion.ToInteger(value.Value);

                    int integerValue;
                    double floatValue;
                    bool isInteger;
                    bool isHexadecimal;
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (isInteger)
                    {
                        if (ArithmeticOperation(integerValue, rightInteger))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (ArithmeticOperation(floatValue, rightInteger))
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
                        if (BitwiseOperation(integerValue, rightInteger))
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

                    base.VisitBooleanValue(value);
                    break;
            }
        }

        public override void VisitStringValue(StringValue value)
        {
            switch (Operation)
            {
                case Operations.Concat:
                    Result = OutSet.CreateString(leftOperand.Value + leftOperand.Value);
                    break;
                default:
                    // TODO: Implement all operations
                    base.VisitStringValue(value);
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
                case Operations.Mod:
                    // TODO: There is a problem with conversion
                    throw new NotImplementedException();
                case Operations.Concat:
                    Result = OutSet.CreateString(leftOperand.Value + TypeConversion.ToString(value.Value));
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
                    var isSuccessful = TypeConversion.TryConvertToNumber(leftOperand.Value, true,
                        out integerValue, out floatValue, out isInteger, out isHexadecimal);

                    if (isInteger)
                    {
                        if (ComparisonOperation(floatValue, value.Value))
                        {
                            break;
                        }

                        if (ArithmeticOperation(floatValue, value.Value))
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (ComparisonOperation(floatValue, value.Value))
                        {
                            break;
                        }

                        if (ArithmeticOperation(floatValue, value.Value))
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
                        if (BitwiseOperation(integerValue, value.Value))
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

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        #endregion
    }
}
