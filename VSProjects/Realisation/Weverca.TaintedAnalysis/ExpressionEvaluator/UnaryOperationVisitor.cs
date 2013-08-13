using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
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

        public FlowOutputSet OutSet
        {
            get { return evaluator.OutSet; }
        }

        public Value Evaluate(Operations unaryOperation, Value operand)
        {
            // Sets current operation
            operation = unaryOperation;

            // Gets type of operand and evaluate expression for given operation
            operand.Accept(this);

            // Returns result of unary operation
            Debug.Assert(result != null, "The result must be assigned after visiting the value");
            return result;
        }

        #region IValueVisitor Members

        public void VisitValue(Value value)
        {
            switch (operation)
            {
                case Operations.UnsetCast:
                    // TODO: Is resolved to null value
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.Int8Cast:
                case Operations.Int16Cast:
                case Operations.Int64Cast:
                case Operations.UInt8Cast:
                case Operations.UInt16Cast:
                case Operations.UInt32Cast:
                case Operations.UInt64Cast:
                case Operations.DecimalCast:
                    throw new NotSupportedException("Cast to different integral types is not supported");
                case Operations.BinaryCast:
                    throw new NotSupportedException("Binary strings are not supported");
                case Operations.AtSign:
                    throw new InvalidOperationException("The references is resolved as special construct");
                default:
                    throw new InvalidOperationException("Resolving of non-unary operation");
            }
        }

        public void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    evaluator.SetWarning("Object could not be converted to int");
                    // TODO: Converting from object to int is undefined
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.Minus:
                    evaluator.SetWarning("Object could not be converted to int");
                    // TODO: Converting from object to int is undefined
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.LogicNegation:
                    throw new NotImplementedException();
                case Operations.BitNegation:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.BoolCast:
                    throw new NotImplementedException();
                case Operations.Int32Cast:
                    throw new NotImplementedException();
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    throw new NotImplementedException();
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    throw new NotImplementedException();
                case Operations.Print:
                    throw new NotImplementedException();
                case Operations.Clone:
                    throw new NotImplementedException();
                case Operations.ObjectCast:
                    result = value;
                    break;
                case Operations.ArrayCast:
                    throw new NotImplementedException();
                default:
                    VisitValue(value);
                    break;
            }
        }

        public void VisitAssociativeArray(AssociativeArray value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    evaluator.SetWarning("Array could not be converted to int");
                    // TODO: Converting from array to int is undefined
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.Minus:
                    evaluator.SetWarning("Array could not be converted to int");
                    // TODO: Converting from array to int is undefined
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.LogicNegation:
                    throw new NotImplementedException();
                case Operations.BitNegation:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.BoolCast:
                    throw new NotImplementedException();
                case Operations.Int32Cast:
                    throw new NotImplementedException();
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    throw new NotImplementedException();
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    TypeConversion.ToString(OutSet, value);
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    throw new NotImplementedException();
                case Operations.ArrayCast:
                    result = value;
                    break;
                default:
                    VisitValue(value);
                    break;
            }
        }

        public void VisitSpecialValue(SpecialValue value)
        {
            VisitValue(value);
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
            switch (operation)
            {
                case Operations.Plus:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.Minus:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.BitNegation:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = TypeConversion.ToFloat(OutSet, value);
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    TypeConversion.ToString(OutSet, value);
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    // TODO: Create new stdClass object
                    throw new NotImplementedException();
                case Operations.ArrayCast:
                    result = OutSet.CreateArray();
                    break;
                default:
                    VisitSpecialValue(value);
                    break;
            }
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

        public void VisitAnyFloatValue(AnyFloatValue value)
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

        public void VisitAnyResourceValue(AnyResourceValue value)
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
            VisitValue(value);
        }

        public void VisitGenericPrimitiveValue<T>(PrimitiveValue<T> value)
        {
            VisitPrimitiveValue(value);
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
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    result = OutSet.CreateDouble(-value.Value);
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(value.Value == 0.0);
                    break;
                case Operations.BitNegation:
                    var number = TypeConversion.ToInteger(OutSet, value);
                    result = OutSet.CreateInt(~number.Value);
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = value;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    TypeConversion.ToString(OutSet, value);
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    // TODO: Create new stdClass object and save value as "scalar" field
                    throw new NotImplementedException();
                case Operations.ArrayCast:
                    // TODO: Create new array with the value at the zero position
                    throw new NotImplementedException();
                default:
                    VisitGenericPrimitiveValue<double>(value);
                    break;
            }
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = OutSet.CreateInt(value.Value ? 1 : 0);
                    break;
                case Operations.Minus:
                    result = OutSet.CreateInt(value.Value ? -1 : 0);
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(!value.Value);
                    break;
                case Operations.BitNegation:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.BoolCast:
                    result = value;
                    break;
                case Operations.Int32Cast:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = TypeConversion.ToFloat(OutSet, value);
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    // TODO: Create new stdClass object and save value as "scalar" field
                    throw new NotImplementedException();
                case Operations.ArrayCast:
                    // TODO: Create new array with the value at the zero position
                    throw new NotImplementedException();
                default:
                    VisitGenericPrimitiveValue<bool>(value);
                    break;
            }
        }

        public void VisitStringValue(StringValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.Minus:
                    var number = TypeConversion.ToInteger(OutSet, value);
                    if ((number.Value == 0) || ((-number.Value) != 0))
                    {
                        result = OutSet.CreateInt(-number.Value);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationVisitor.VisitStringValue"/>
                        result = OutSet.CreateDouble(-(System.Convert.ToDouble(number.Value)));
                    }
                    break;
                case Operations.LogicNegation:
                    var boolean = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!boolean.Value);
                    break;
                case Operations.BitNegation:
                    // TODO: Is this defined?
                    throw new NotImplementedException();
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = TypeConversion.ToFloat(OutSet, value);
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = value;
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    // TODO: Create new stdClass object and save value as "scalar" field
                    throw new NotImplementedException();
                case Operations.ArrayCast:
                    // TODO: Create new array with the value at the zero position
                    throw new NotImplementedException();
                default:
                    VisitGenericPrimitiveValue<string>(value);
                    break;
            }
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
                        result = OutSet.CreateInt(-value.Value);
                    }
                    else
                    {
                        // If the number has the lowest value (all 1s in binary), negation of it
                        // is the same value. PHP behaves differently. It converts the number
                        // to the same positive value, but that cause overflow. Then integer value
                        // is converted to appropriate double value
                        result = OutSet.CreateDouble(-(System.Convert.ToDouble(value.Value)));
                    }
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(value.Value == 0);
                    break;
                case Operations.BitNegation:
                    result = OutSet.CreateInt(~value.Value);
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    result = value;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = TypeConversion.ToFloat(OutSet, value);
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    TypeConversion.ToString(OutSet, value);
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    // TODO: Create new stdClass object and save value as "scalar" field
                    throw new NotImplementedException();
                case Operations.ArrayCast:
                    // TODO: Create new array with the value at the zero position
                    throw new NotImplementedException();
                default:
                    VisitGenericPrimitiveValue<int>(value);
                    break;
            }
        }

        public void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            VisitValue(value);
        }

        public void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            VisitGenericIntervalValue<int>(value);
        }

        public void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            throw new NotSupportedException();
        }

        public void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            VisitGenericIntervalValue<double>(value);
        }

        #endregion
    }
}
