using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    public class UnaryOperationVisitor : IValueVisitor
    {
        private ExpressionEvaluator evaluator;
        private Operations operation;
        private Value result;

        public UnaryOperationVisitor(ExpressionEvaluator expressionEvaluator)
        {
            evaluator = expressionEvaluator;
        }

        public Value Result { get; private set; }

        public Value Evaluate(Operations unaryOperation, Value operand)
        {
            // Sets current operation
            operation = unaryOperation;

            // Gets type of operand and evaluate expression for given operation
            operand.Accept(this);

            // Returns result of unary operation
            Debug.Assert(result != null, "The rusult must be assigned after visiting the value");
            return result;
        }

        #region IValueVisitor Members

        public void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectValue(ObjectValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAssociativeArray(AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        public void VisitSpecialValue(SpecialValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAliasValue(AliasValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyValue(AnyValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitUndefinedValue(UndefinedValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyStringValue(AnyStringValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyLongintValue(AnyLongintValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyObjectValue(AnyObjectValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyArrayValue(AnyArrayValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitInfoValue(InfoValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitInfoValue<T>(InfoValue<T> value)
        {
            throw new NotImplementedException();
        }

        public void VisitPrimitiveValue(PrimitiveValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitGenericPrimitiveValue<T>(PrimitiveValue<T> value)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionValue(FunctionValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitTypeValue(TypeValue typeValue)
        {
            throw new NotImplementedException();
        }

        public void VisitFloatValue(FloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitStringValue(StringValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    if ((value.Value == 0) || ((-value.Value) != 0))
                    {
                        result = evaluator.OutSet.CreateInt(-value.Value);
                    }
                    else
                    {
                        // If the number has the lowest value (all 1s in binary), negation of it
                        // is the same value. PHP behaves differently. It converts the number
                        // to the same positive value, but that cause overflow. Then integer value
                        // is converted to appropriate double value
                        result = evaluator.OutSet.CreateDouble(-((double)value.Value));
                    }
                    break;
                case Operations.LogicNegation:
                    result = evaluator.OutSet.CreateBool(value.Value == 0);
                    break;
                case Operations.BitNegation:
                    result = evaluator.OutSet.CreateInt(~value.Value);
                    break;
                case Operations.Int8Cast:
                case Operations.Int16Cast:
                case Operations.Int64Cast:
                case Operations.UInt8Cast:
                case Operations.UInt16Cast:
                case Operations.UInt32Cast:
                case Operations.UInt64Cast:
                case Operations.DecimalCast:
                    Debug.Fail("Cast to different integral types is not supported");
                    result = value;
                    break;
                case Operations.Int32Cast:
                    result = value;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = evaluator.OutSet.CreateDouble(System.Convert.ToDouble(value.Value));
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    // TODO: Use defined function
                    result = evaluator.OutSet.CreateString(value.Value.ToString());
                    break;
                case Operations.BoolCast:
                    result = evaluator.OutSet.CreateBool(value.Value != 0);
                    break;
                case Operations.UnsetCast:
                    // TOOD: It should destroy the variable. However, the name of variable is not know
                    // inside UnaryEx method, the variable can have more name and values too.
                    // TODO: Is resolved to null value
                    result = evaluator.OutSet.UndefinedValue;
                    break;
                case Operations.Clone:
                    // TODO: Fatal Error
                    Debug.Fail("Cloning of number causes fatal error");
                    break;
                case Operations.Print:
                    // TODO: This is a quest for tainted analysis
                    result = evaluator.OutSet.CreateBool(false);
                    break;
                case Operations.ObjectCast:
                case Operations.ArrayCast:
                case Operations.BinaryCast:

                case Operations.AtSign:

                default:
                    throw new NotImplementedException();
            }
        }

        public void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            VisitGenericIntervalValue<int>(value);
        }

        public void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            VisitGenericIntervalValue<long>(value);
        }

        public void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            VisitGenericIntervalValue<double>(value);
        }

        #endregion


        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }
    }
}
