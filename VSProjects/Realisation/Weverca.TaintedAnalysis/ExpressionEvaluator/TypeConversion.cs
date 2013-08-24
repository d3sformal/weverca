using System;

using PHP.Core;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    public static class TypeConversion
    {
        public static BooleanValue ToBoolean(FlowOutputSet output, IntegerValue value)
        {
            return output.CreateBool(value.Value != 0);
        }

        public static BooleanValue ToBoolean(FlowOutputSet output, LongintValue value)
        {
            return output.CreateBool(value.Value != 0);
        }

        public static BooleanValue ToBoolean(FlowOutputSet output, FloatValue value)
        {
            return output.CreateBool(value.Value != 0.0);
        }

        public static BooleanValue ToBoolean(FlowOutputSet output, StringValue value)
        {
            Debug.Assert(value.Value != null, "StringValue can never be null");
            return output.CreateBool((value.Value.Length != 0) && (!string.Equals(value.Value, "0")));
        }

        public static BooleanValue ToBoolean(FlowOutputSet output, AnyResourceValue value)
        {
            return output.CreateBool(true);
        }

        public static BooleanValue ToBoolean(FlowOutputSet output, UndefinedValue value)
        {
            return output.CreateBool(false);
        }

        public static IntegerValue ToInteger(FlowOutputSet output, BooleanValue value)
        {
            return output.CreateInt(System.Convert.ToInt32(value.Value));
        }

        public static IntegerValue ToInteger(FlowOutputSet output, FloatValue value)
        {
            var truncated = Math.Truncate(value.Value);
            int casted;
            try
            {
                casted = System.Convert.ToInt32(truncated);
            }
            catch (OverflowException)
            {
                // TODO: Pridat varovani a nastavit na nejakou hodnotu
                casted = 0;
            }

            return output.CreateInt(casted);
        }

        public static IntegerValue ToInteger(FlowOutputSet output, StringValue value)
        {
            Debug.Assert(value.Value != null, "StringValue can never be null");
            // TODO: Implement the correct convert to integer
            int result;
            if (int.TryParse(value.Value, out result))
            {
                return output.CreateInt(result);
            }
            else
            {
                return output.CreateInt(0);
            }
        }

        public static IntegerValue ToInteger(FlowOutputSet output, UndefinedValue value)
        {
            return output.CreateInt(0);
        }

        public static FloatValue ToFloat(FlowOutputSet output, BooleanValue value)
        {
            return output.CreateDouble(System.Convert.ToDouble(value.Value));
        }

        public static FloatValue ToFloat(FlowOutputSet output, IntegerValue value)
        {
            return output.CreateDouble(System.Convert.ToDouble(value.Value));
        }

        public static FloatValue ToFloat(FlowOutputSet output, StringValue value)
        {
            Debug.Assert(value.Value != null, "StringValue can never be null");
            // TODO: Implement the correct convert to float
            double result;
            if (double.TryParse(value.Value, out result))
            {
                return output.CreateDouble(result);
            }
            else
            {
                return output.CreateDouble(0.0);
            }
        }

        public static FloatValue ToFloat(FlowOutputSet output, UndefinedValue value)
        {
            return output.CreateDouble(0.0);
        }

        public static StringValue ToString(FlowOutputSet output, BooleanValue value)
        {
            return output.CreateString(value.Value ? "1" : string.Empty);
        }

        public static StringValue ToString(FlowOutputSet output, IntegerValue value)
        {
            return output.CreateString(System.Convert.ToString(value.Value));
        }

        public static StringValue ToString(FlowOutputSet output, LongintValue value)
        {
            return output.CreateString(System.Convert.ToString(value.Value));
        }

        public static StringValue ToString(FlowOutputSet output, FloatValue value)
        {
            return output.CreateString(System.Convert.ToString(value.Value));
        }

        public static StringValue ToString(FlowOutputSet output, AssociativeArray value)
        {
            return output.CreateString("Array");
        }

        public static StringValue ToString(FlowOutputSet output, UndefinedValue value)
        {
            return output.CreateString(string.Empty);
        }

        public static AssociativeArray ToArray(FlowOutputSet output, UndefinedValue value)
        {
            return output.CreateArray();
        }
    }


    class ValueTypeResolver
    {
        public static bool isInt(Value value)
        {
            return (value is IntegerIntervalValue || value is IntegerValue || value is AnyIntegerValue);
        }

        public static bool isLong(Value value)
        {
            return (value is LongintValue || value is AnyLongintValue || value is LongintIntervalValue);
        }

        public static bool isFloat(Value value)
        {
            return (value is FloatIntervalValue || value is FloatValue || value is AnyFloatValue);
        }

        public static bool isBool(Value value)
        {
            return (value is BooleanValue || value is AnyBooleanValue);
        }

        public static bool isString(Value value)
        {
            return (value is StringValue || value is AnyStringValue);
        }

        public static bool isObject(Value value)
        {
            return (value is ObjectValue || value is AnyObjectValue);
        }

        public static bool isArray(Value value)
        {
            return (value is AssociativeArray || value is AnyArrayValue);
        }

        public static bool CanBeDirty(Value value)
        {
            if (ValueTypeResolver.isBool(value) || ValueTypeResolver.isInt(value) || ValueTypeResolver.isFloat(value) || ValueTypeResolver.isLong(value))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }


}
