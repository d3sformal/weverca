using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    public class UnaryOperationVisitor : AbstractValueVisitor
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

        public override void VisitValue(Value value)
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
                    evaluator.SetWarning("Try to suppress a warning of the expression");
                    result = value;
                    break;
                default:
                    throw new InvalidOperationException("Resolving of non-unary operation");
            }
        }

        public override void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    evaluator.SetWarning("Object cannot be converted to int");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Minus:
                    evaluator.SetWarning("Object cannot be converted to int");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.LogicNegation:
                    // Every object can be converted to true value
                    result = OutSet.CreateBool(false);
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
                    evaluator.SetWarning("Object cannot be converted to int");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    evaluator.SetWarning("Object cannot be converted to float");
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    throw new NotImplementedException();
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    throw new NotImplementedException();
                case Operations.Clone:
                    // TODO: Object can by converted only if it has __clone magic method implemented
                    throw new NotImplementedException();
                case Operations.ObjectCast:
                    result = value;
                    break;
                case Operations.ArrayCast:
                    result = TypeConversion.ToArray(OutSet, value);
                    break;
                default:
                    base.VisitObjectValue(value);
                    break;
            }
        }

        public override void VisitAssociativeArray(AssociativeArray value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.Minus:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.LogicNegation:
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
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
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Clone:
                    // TODO: This is be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.ObjectCast:
                    result = TypeConversion.ToObject(OutSet, value);
                    break;
                case Operations.ArrayCast:
                    result = value;
                    break;
                default:
                    base.VisitAssociativeArray(value);
                    break;
            }
        }

        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.Minus:
                    result = TypeConversion.ToInteger(OutSet, value);
                    break;
                case Operations.LogicNegation:
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
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
                default:
                    if (!PerformUsualOperation(value))
                    {
                        base.VisitUndefinedValue(value);
                    }
                    break;
            }
        }

        public override void VisitResourceValue(ResourceValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Minus:
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.LogicNegation:
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
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
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                default:
                    if (!PerformUsualOperation(value))
                    {
                        base.VisitResourceValue(value);
                    }
                    break;
            }
        }

        public override void VisitAnyPrimitiveValue(AnyPrimitiveValue value)
        {
            switch (operation)
            {
                case Operations.BoolCast:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Int32Cast:
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = OutSet.AnyStringValue;
                    break;
                default:
                    if (!PerformUsualOperation(value))
                    {
                        base.VisitAnyPrimitiveValue(value);
                    }
                    break;
            }
        }

        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    result = value;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitNegation:
                    result = OutSet.AnyIntegerValue;
                    break;
                default:
                    base.VisitAnyFloatValue(value);
                    break;
            }
        }

        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = OutSet.CreateIntegerInterval(0, 1);
                    break;
                case Operations.Minus:
                    result = OutSet.CreateIntegerInterval(-1, 0);
                    break;
                case Operations.LogicNegation:
                    result = value;
                    break;
                case Operations.BitNegation:
                    // TODO: This is fatal error
                    evaluator.SetWarning("Unsupported operand types");
                    result = OutSet.UndefinedValue;
                    break;
                default:
                    base.VisitAnyBooleanValue(value);
                    break;
            }
        }

        public override void VisitAnyStringValue(AnyStringValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Minus:
                    // It can be integer or double
                    result = OutSet.AnyValue;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitNegation:
                    // Bit negation is defined for every character, not for the entire string
                    result = value;
                    break;
                default:
                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    // It can be long integer or double
                    result = OutSet.AnyValue;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitNegation:
                    result = value;
                    break;
                default:
                    base.VisitAnyLongintValue(value);
                    break;
            }
        }

        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    // It can be integer or double
                    result = OutSet.AnyValue;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitNegation:
                    result = value;
                    break;
                default:
                    base.VisitAnyIntegerValue(value);
                    break;
            }
        }

        public override void VisitPrimitiveValue(PrimitiveValue value)
        {
            if (!PerformUsualOperation(value))
            {
                base.VisitPrimitiveValue(value);
            }
        }

        public override void VisitFloatValue(FloatValue value)
        {
            IntegerValue convertedValue;

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
                    if (TypeConversion.TryConvertToInteger(OutSet, value, out convertedValue))
                    {
                        result = OutSet.CreateInt(~convertedValue.Value);
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    if (TypeConversion.TryConvertToInteger(OutSet, value, out convertedValue))
                    {
                        result = convertedValue;
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = value;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                default:
                    base.VisitFloatValue(value);
                    break;
            }
        }

        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = TypeConversion.ToInteger(OutSet, value);
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
                default:
                    base.VisitBooleanValue(value);
                    break;
            }
        }

        public override void VisitStringValue(StringValue value)
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
                        // <seealso cref="UnaryOperationVisitor.VisitIntegerValue"/>
                        result = OutSet.CreateDouble(-(System.Convert.ToDouble(number.Value)));
                    }
                    break;
                case Operations.LogicNegation:
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
                    break;
                case Operations.BitNegation:
                    // Bit negation is defined for every character, not for the entire string
                    // TODO: Implement
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
                default:
                    base.VisitStringValue(value);
                    break;
            }
        }

        public override void VisitLongintValue(LongintValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    if ((value.Value == 0) || ((-value.Value) != 0))
                    {
                        result = OutSet.CreateLong(-value.Value);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationVisitor.VisitIntegerValue"/>
                        result = OutSet.CreateDouble(-(System.Convert.ToDouble(value.Value)));
                    }
                    break;
                case Operations.LogicNegation:
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
                    break;
                case Operations.BitNegation:
                    result = OutSet.CreateLong(~value.Value);
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    IntegerValue convertedValue;
                    if (TypeConversion.TryConvertToInteger(OutSet, value, out convertedValue))
                    {
                        result = convertedValue;
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = TypeConversion.ToFloat(OutSet, value);
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                default:
                    base.VisitLongintValue(value);
                    break;
            }
        }

        public override void VisitIntegerValue(IntegerValue value)
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
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
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
                default:
                    base.VisitIntegerValue(value);
                    break;
            }
        }

        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            BooleanValue booleanValue;

            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.LogicNegation:
                    if (TypeConversion.TryConvertToBoolean<T>(OutSet, value, out booleanValue))
                    {
                        result = OutSet.CreateBool(!booleanValue.Value);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    break;
                case Operations.BoolCast:
                    if (TypeConversion.TryConvertToBoolean<T>(OutSet, value, out booleanValue))
                    {
                        result = booleanValue;
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    result = OutSet.AnyStringValue;
                    break;
                default:
                    if (!PerformUsualOperation(value))
                    {
                        base.VisitGenericIntervalValue(value);
                    }
                    break;
            }
        }

        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            if (value.Start.Equals(value.End))
            {
                result = OutSet.CreateInt(value.Start);
                return;
            }

            switch (operation)
            {
                case Operations.Minus:
                    if ((value.Start == 0) || ((-value.Start) != 0))
                    {
                        result = OutSet.CreateIntegerInterval(-value.End, -value.Start);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationVisitor.VisitIntegerValue"/>
                        result = OutSet.CreateFloatInterval(-System.Convert.ToDouble(value.End),
                            -System.Convert.ToDouble(value.Start));
                    }
                    break;
                case Operations.BitNegation:
                    result = OutSet.CreateIntegerInterval(~value.End, ~value.Start);
                    break;
                case Operations.Int32Cast:
                    result = value;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = OutSet.CreateFloatInterval(System.Convert.ToDouble(value.Start),
                        System.Convert.ToDouble(value.End));
                    break;
                default:
                    base.VisitIntervalIntegerValue(value);
                    break;
            }
        }

        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            if (value.Start.Equals(value.End))
            {
                result = OutSet.CreateLong(value.Start);
                return;
            }

            switch (operation)
            {
                case Operations.Minus:
                    if ((value.Start == 0) || ((-value.Start) != 0))
                    {
                        result = OutSet.CreateLongintInterval(-value.End, -value.Start);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationVisitor.VisitIntegerValue"/>
                        result = OutSet.CreateFloatInterval(-System.Convert.ToDouble(value.End),
                            -System.Convert.ToDouble(value.Start));
                    }
                    break;
                case Operations.BitNegation:
                    result = OutSet.CreateLongintInterval(~value.End, ~value.Start);
                    break;
                case Operations.Int32Cast:
                    IntegerIntervalValue integerInterval;
                    if (TypeConversion.TryConvertToIntegerInterval(OutSet, value, out integerInterval))
                    {
                        result = integerInterval;
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = OutSet.CreateFloatInterval(System.Convert.ToDouble(value.Start),
                        System.Convert.ToDouble(value.End));
                    break;
                default:
                    base.VisitIntervalLongintValue(value);
                    break;
            }
        }

        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            if (value.Start.Equals(value.End))
            {
                result = OutSet.CreateDouble(value.Start);
                return;
            }

            IntegerIntervalValue integerInterval;

            switch (operation)
            {
                case Operations.Minus:
                    result = OutSet.CreateFloatInterval(-value.End, -value.Start);
                    break;
                case Operations.BitNegation:
                    if (TypeConversion.TryConvertToIntegerInterval(OutSet, value, out integerInterval))
                    {
                        result = OutSet.CreateIntegerInterval(~integerInterval.End, ~integerInterval.Start);
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Int32Cast:
                    if (TypeConversion.TryConvertToIntegerInterval(OutSet, value, out integerInterval))
                    {
                        result = integerInterval;
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = value;
                    break;
                default:
                    base.VisitIntervalFloatValue(value);
                    break;
            }
        }

        #endregion

        #region Helper methods

        private bool PerformUsualOperation(Value value)
        {
            switch (operation)
            {
                case Operations.Clone:
                    // TODO: This is be fatal error
                    evaluator.SetWarning("__clone method called on non-object");
                    result = OutSet.UndefinedValue;
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.ObjectCast:
                    result = TypeConversion.ToObject(OutSet, value);
                    break;
                case Operations.ArrayCast:
                    result = TypeConversion.ToObject(OutSet, value);
                    break;
                default:
                    return false;
            }

            return true;
        }

        #endregion
    }
}
