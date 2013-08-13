using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    public static class BinaryOperations
    {
        public static Value BinaryOperation(Value leftOperand, Value rightOperand)
        {
            throw new NotImplementedException();
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, BooleanValue leftOperand,
            Operations operation, BooleanValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(leftOperand.Value == rightOperand.Value);
                case Operations.NotEqual:
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(leftOperand.Value != rightOperand.Value);
                case Operations.LessThan:
                    return evaluator.OutSet.CreateBool((!leftOperand.Value) && rightOperand.Value);
                case Operations.LessThanOrEqual:
                    return evaluator.OutSet.CreateBool((!leftOperand.Value) || rightOperand.Value);
                case Operations.GreaterThan:
                    return evaluator.OutSet.CreateBool(leftOperand.Value && (!rightOperand.Value));
                case Operations.GreaterThanOrEqual:
                    return evaluator.OutSet.CreateBool(leftOperand.Value || (!rightOperand.Value));
                case Operations.Add:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value) + System.Convert.ToInt32(rightOperand.Value));
                case Operations.Sub:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value) - System.Convert.ToInt32(rightOperand.Value));
                case Operations.Mul:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value && rightOperand.Value));
                case Operations.Div:
                    if (rightOperand.Value)
                    {
                        return TypeConversion.ToInteger(evaluator.OutSet, leftOperand);
                    }
                    else
                    {
                        // TODO: Return warning value
                        // Division by false returns false boolean value
                        return evaluator.OutSet.CreateBool(false);
                    }
                case Operations.Mod:
                    if (rightOperand.Value)
                    {
                        return evaluator.OutSet.CreateInt(System.Convert.ToInt32(false));
                    }
                    else
                    {
                        // TODO: Return warning value
                        // Division by false returns false boolean value
                        return evaluator.OutSet.CreateBool(false);
                    }
                case Operations.BitAnd:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value && rightOperand.Value));
                case Operations.BitOr:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value || rightOperand.Value));
                case Operations.BitXor:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value != rightOperand.Value));
                case Operations.ShiftLeft:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value) << System.Convert.ToInt32(rightOperand.Value));
                case Operations.ShiftRight:
                    return evaluator.OutSet.CreateInt(System.Convert.ToInt32(leftOperand.Value) >> System.Convert.ToInt32(rightOperand.Value));
                case Operations.And:
                    return evaluator.OutSet.CreateBool(leftOperand.Value && rightOperand.Value);
                case Operations.Or:
                    return evaluator.OutSet.CreateBool(leftOperand.Value || rightOperand.Value);
                case Operations.Xor:
                    return evaluator.OutSet.CreateBool(leftOperand.Value != rightOperand.Value);
                case Operations.Concat:
                    return evaluator.OutSet.CreateString((leftOperand.Value ? "1" : string.Empty) + (rightOperand.Value ? "1" : string.Empty));
                default:
                    Debug.Fail("There is no other binary operation between integers!");
                    return evaluator.OutSet.AnyValue;
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, BooleanValue leftOperand,
            Operations operation, IntegerValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(false);
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(true);
                default:
                    var value = TypeConversion.ToInteger(evaluator.OutSet, leftOperand);
                    return BinaryOperation(evaluator, value, operation, rightOperand);
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, BooleanValue leftOperand,
            Operations operation, FloatValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(false);
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(true);
                default:
                    var value = TypeConversion.ToInteger(evaluator.OutSet, leftOperand);
                    return BinaryOperation(evaluator, value, operation, rightOperand);
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, IntegerValue leftOperand,
            Operations operation, BooleanValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(false);
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(true);
                default:
                    var value = TypeConversion.ToInteger(evaluator.OutSet, rightOperand);
                    return BinaryOperation(evaluator, leftOperand, operation, value);
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, IntegerValue leftOperand,
            Operations operation, IntegerValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(leftOperand.Value == rightOperand.Value);
                case Operations.NotEqual:
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(leftOperand.Value != rightOperand.Value);
                case Operations.LessThan:
                    return evaluator.OutSet.CreateBool(leftOperand.Value < rightOperand.Value);
                case Operations.LessThanOrEqual:
                    return evaluator.OutSet.CreateBool(leftOperand.Value <= rightOperand.Value);
                case Operations.GreaterThan:
                    return evaluator.OutSet.CreateBool(leftOperand.Value > rightOperand.Value);
                case Operations.GreaterThanOrEqual:
                    return evaluator.OutSet.CreateBool(leftOperand.Value >= rightOperand.Value);
                case Operations.Add:
                    try
                    {
                        int sum;
                        checked
                        {
                            // Maximal integer value is PHP.Core.Reflection.GlobalConstant.PhpIntMax.Value
                            sum = leftOperand.Value + rightOperand.Value;
                        }
                        return evaluator.OutSet.CreateInt(sum);
                    }
                    catch (OverflowException)
                    {
                        // If aritmetic overflows, result is double
                        return evaluator.OutSet.CreateDouble((double)leftOperand.Value + rightOperand.Value);
                    }
                case Operations.Sub:
                    try
                    {
                        int difference;
                        checked
                        {
                            difference = leftOperand.Value - rightOperand.Value;
                        }
                        return evaluator.OutSet.CreateInt(difference);
                    }
                    catch (OverflowException)
                    {
                        // If aritmetic overflows, result is double
                        return evaluator.OutSet.CreateDouble((double)leftOperand.Value - rightOperand.Value);
                    }
                case Operations.Mul:
                    try
                    {
                        int product;
                        checked
                        {
                            product = leftOperand.Value * rightOperand.Value;
                        }
                        return evaluator.OutSet.CreateInt(product);
                    }
                    catch (OverflowException)
                    {
                        // If aritmetic overflows, result is double
                        return evaluator.OutSet.CreateDouble((double)leftOperand.Value * rightOperand.Value);
                    }
                case Operations.Div:
                    if (rightOperand.Value != 0)
                    {
                        return evaluator.OutSet.CreateDouble((double)leftOperand.Value / rightOperand.Value);
                    }
                    else
                    {
                        // TODO: Return warning value
                        // Division by zero returns false boolean value
                        return evaluator.OutSet.CreateBool(false);
                    }
                case Operations.Mod:
                    if (rightOperand.Value != 0)
                    {
                        // Value has the same sign as dividend
                        return evaluator.OutSet.CreateInt(leftOperand.Value % rightOperand.Value);
                    }
                    else
                    {
                        // TODO: Return warning value
                        // Division by zero returns false boolean value
                        return evaluator.OutSet.CreateBool(false);
                    }
                case Operations.BitAnd:
                    return evaluator.OutSet.CreateInt(leftOperand.Value & rightOperand.Value);
                case Operations.BitOr:
                    return evaluator.OutSet.CreateInt(leftOperand.Value | rightOperand.Value);
                case Operations.BitXor:
                    return evaluator.OutSet.CreateInt(leftOperand.Value ^ rightOperand.Value);
                case Operations.ShiftLeft:
                    return evaluator.OutSet.CreateInt(leftOperand.Value << rightOperand.Value);
                case Operations.ShiftRight:
                    return evaluator.OutSet.CreateInt(leftOperand.Value >> rightOperand.Value);
                case Operations.And:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0) && (rightOperand.Value != 0));
                case Operations.Or:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0) || (rightOperand.Value != 0));
                case Operations.Xor:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0) != (rightOperand.Value != 0));
                case Operations.Concat:
                    return evaluator.OutSet.CreateString(leftOperand.Value.ToString() + rightOperand.Value.ToString());
                default:
                    Debug.Fail("There is no other binary operation between integers!");
                    return evaluator.OutSet.AnyValue;
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, IntegerValue leftOperand,
            Operations operation, FloatValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(false);
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(true);
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                case Operations.Add:
                case Operations.Sub:
                case Operations.Mul:
                case Operations.Div:
                    var floatValue = TypeConversion.ToFloat(evaluator.OutSet, leftOperand);
                    return BinaryOperation(evaluator, floatValue, operation, rightOperand);
                case Operations.Mod:
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    var intValue = TypeConversion.ToInteger(evaluator.OutSet, rightOperand);
                    return BinaryOperation(evaluator, leftOperand, operation, intValue);
                case Operations.And:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0) && (rightOperand.Value != 0.0));
                case Operations.Or:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0) || (rightOperand.Value != 0.0));
                case Operations.Xor:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0) != (rightOperand.Value != 0.0));
                case Operations.Concat:
                    return evaluator.OutSet.CreateString(leftOperand.Value.ToString() + rightOperand.Value.ToString());
                default:
                    Debug.Fail("There are no other binary operators!");
                    return null;
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, FloatValue leftOperand,
            Operations operation, BooleanValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(false);
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(true);
                default:
                    var value = TypeConversion.ToInteger(evaluator.OutSet, rightOperand);
                    return BinaryOperation(evaluator, leftOperand, operation, value);
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, FloatValue leftOperand,
            Operations operation, IntegerValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Identical:
                    return evaluator.OutSet.CreateBool(false);
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(true);
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.LessThan:
                case Operations.LessThanOrEqual:
                case Operations.GreaterThan:
                case Operations.GreaterThanOrEqual:
                case Operations.Add:
                case Operations.Sub:
                case Operations.Mul:
                case Operations.Div:
                    var floatValue = TypeConversion.ToFloat(evaluator.OutSet, rightOperand);
                    return BinaryOperation(evaluator, leftOperand, operation, floatValue);
                case Operations.Mod:
                case Operations.BitAnd:
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    var intValue = TypeConversion.ToInteger(evaluator.OutSet, leftOperand);
                    return BinaryOperation(evaluator, intValue, operation, rightOperand);
                case Operations.And:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0.0) && (rightOperand.Value != 0));
                case Operations.Or:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0.0) || (rightOperand.Value != 0));
                case Operations.Xor:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0.0) != (rightOperand.Value != 0));
                case Operations.Concat:
                    return evaluator.OutSet.CreateString(leftOperand.Value.ToString() + rightOperand.Value.ToString());
                default:
                    Debug.Fail("There are no other binary operators!");
                    return null;
            }
        }

        public static Value BinaryOperation(ExpressionEvaluator evaluator, FloatValue leftOperand,
            Operations operation, FloatValue rightOperand)
        {
            switch (operation)
            {
                case Operations.Equal:
                case Operations.Identical:
                    // TODO: It should return warning, floats should not be compared for equality
                    return evaluator.OutSet.CreateBool(leftOperand.Value == rightOperand.Value);
                case Operations.NotEqual:
                case Operations.NotIdentical:
                    return evaluator.OutSet.CreateBool(leftOperand.Value != rightOperand.Value);
                case Operations.LessThan:
                    return evaluator.OutSet.CreateBool(leftOperand.Value < rightOperand.Value);
                case Operations.LessThanOrEqual:
                    return evaluator.OutSet.CreateBool(leftOperand.Value <= rightOperand.Value);
                case Operations.GreaterThan:
                    return evaluator.OutSet.CreateBool(leftOperand.Value > rightOperand.Value);
                case Operations.GreaterThanOrEqual:
                    return evaluator.OutSet.CreateBool(leftOperand.Value >= rightOperand.Value);
                case Operations.Add:
                    return evaluator.OutSet.CreateDouble(leftOperand.Value + rightOperand.Value);
                case Operations.Sub:
                    return evaluator.OutSet.CreateDouble(leftOperand.Value - rightOperand.Value);
                case Operations.Mul:
                    return evaluator.OutSet.CreateDouble(leftOperand.Value * rightOperand.Value);
                case Operations.Div:
                    if (rightOperand.Value != 0.0)
                    {
                        return evaluator.OutSet.CreateDouble(leftOperand.Value / rightOperand.Value);
                    }
                    else
                    {
                        // TODO: Return warning value
                        // Division by zero does not return NaN, but false boolean value
                        return evaluator.OutSet.CreateBool(false);
                    }
                case Operations.Mod:
                    var dividend = TypeConversion.ToInteger(evaluator.OutSet, leftOperand);
                    var divisor = TypeConversion.ToInteger(evaluator.OutSet, rightOperand);
                    if (divisor.Value != 0)
                    {
                        // Value has the same sign as dividend
                        return evaluator.OutSet.CreateInt(dividend.Value % divisor.Value);
                    }
                    else
                    {
                        // TODO: Return warning value
                        // Division by zero returns false boolean value
                        return evaluator.OutSet.CreateBool(false);
                    }
                // TODO: Vsechny bitove operace nejdrive prevadeji na int
                case Operations.BitAnd:
                    return evaluator.OutSet.CreateDouble((int)leftOperand.Value & (int)rightOperand.Value);
                case Operations.BitOr:
                    return evaluator.OutSet.CreateDouble((int)leftOperand.Value | (int)rightOperand.Value);
                case Operations.BitXor:
                    return evaluator.OutSet.CreateDouble((int)leftOperand.Value ^ (int)rightOperand.Value);
                case Operations.ShiftLeft:
                    return evaluator.OutSet.CreateDouble((int)leftOperand.Value << (int)rightOperand.Value);
                case Operations.ShiftRight:
                    return evaluator.OutSet.CreateDouble((int)leftOperand.Value >> (int)rightOperand.Value);

                case Operations.And:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0.0) && (rightOperand.Value != 0.0));
                case Operations.Or:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0.0) || (rightOperand.Value != 0.0));
                case Operations.Xor:
                    return evaluator.OutSet.CreateBool((leftOperand.Value != 0.0) != (rightOperand.Value != 0.0));
                case Operations.Concat:
                    return evaluator.OutSet.CreateString(leftOperand.Value.ToString() + rightOperand.Value.ToString());
                default:
                    Debug.Fail("There are no other binary operators!");
                    return null;
            }
        }
    }
}
