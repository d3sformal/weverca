using System;

using PHP.Core;

using Weverca.Analysis;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts values of various PHP data types to another types.
    /// </summary>
    public static class TypeConversion
    {
        #region ToBoolean

        /// <summary>
        /// Converts the value of integer to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Integer to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, IntegerValue value)
        {
            return outset.CreateBool(value.Value != 0);
        }

        /// <summary>
        /// Converts the value of long integer to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Long integer to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, LongintValue value)
        {
            return outset.CreateBool(value.Value != 0);
        }

        /// <summary>
        /// Converts the value of floating-point number to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, FloatValue value)
        {
            return outset.CreateBool(value.Value != 0.0);
        }

        /// <summary>
        /// Converts the string value to proper boolean value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">String to convert</param>
        /// <returns><c>true</c> if string is not empty or "0", otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, StringValue value)
        {
            Debug.Assert(value.Value != null, "StringValue can never be null");
            return outset.CreateBool((value.Value.Length != 0) && (!string.Equals(value.Value, "0")));
        }

        /// <summary>
        /// Determines boolean value from the object reference value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Object of any type to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, ObjectValue value)
        {
            // Notice that in PHP 4, an object evaluates as false if it has no properties.
            return outset.CreateBool(true);
        }

        /// <summary>
        /// Determines boolean value from content of the array value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Array to convert</param>
        /// <returns><c>true</c> if array has at least one element, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, AssociativeArray value)
        {
            var indices = outset.IterateArray(value);
            var enumerator = indices.GetEnumerator();
            return outset.CreateBool(enumerator.MoveNext());
        }

        /// <summary>
        /// Determines boolean value from the reference to external resource.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">External resource to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, ResourceValue value)
        {
            return outset.CreateBool(true);
        }

        /// <summary>
        /// Determines boolean value from any object reference value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Any object of any type to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, AnyObjectValue value)
        {
            // Notice that in PHP 4, an object evaluates as false if it has no properties.
            return outset.CreateBool(true);
        }

        /// <summary>
        /// Determines boolean value from any reference to external resource.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Any external resource to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, AnyResourceValue value)
        {
            return outset.CreateBool(true);
        }

        /// <summary>
        /// Tries to convert possible interval of numbers to an equivalent boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval</typeparam>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Value representing interval of numbers to convert</param>
        /// <param name="convertedValue">
        /// <c>true</c> if interval does not contain zero and
        /// <c>false</c> if interval consists only from zero value or other cases.
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToBoolean<T>(FlowOutputSet outset, IntervalValue<T> value,
            out BooleanValue convertedValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((value.Start.CompareTo(value.Zero) <= 0) && (value.End.CompareTo(value.Zero) >= 0))
            {
                if (value.Start.Equals(value.Zero) && value.End.Equals(value.Zero))
                {
                    convertedValue = outset.CreateBool(false);
                    return true;
                }
                else
                {
                    convertedValue = outset.CreateBool(false);
                    return false;
                }
            }
            else
            {
                convertedValue = outset.CreateBool(true);
                return true;
            }
        }

        /// <summary>
        /// Converts an undefined value to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Always <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateBool(false);
        }

        #endregion

        #region ToInteger

        /// <summary>
        /// Converts the boolean value to an equivalent value of integer.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Boolean value to convert</param>
        /// <returns>The number 1 if value is <c>true</c>, otherwise 0</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, BooleanValue value)
        {
            return outset.CreateInt(System.Convert.ToInt32(value.Value));
        }

        /// <summary>
        /// Tries to convert the value of long integer to an equivalent integer value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Long integer to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, LongintValue value,
            out IntegerValue convertedValue)
        {
            int casted;
            var isConverted = TryConvertToInteger(outset, value.Value, out casted);
            convertedValue = outset.CreateInt(casted);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the value of floating-point number to an equivalent integer value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(FlowOutputSet, double, out int)"/>
        /// </remarks>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, FloatValue value,
            out IntegerValue convertedValue)
        {
            int casted;
            var isConverted = TryConvertToInteger(outset, value.Value, out casted);
            convertedValue = outset.CreateInt(casted);
            return isConverted;
        }

        /// <summary>
        /// Converts the string value to corresponding integer value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">String to convert</param>
        /// <returns>Integer representation of string if it can be converted, otherwise 0</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, StringValue value)
        {
            IntegerValue convertedValue;
            TryConvertToInteger(outset, value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Tries to convert the string value to corresponding integer value.
        /// </summary>
        /// <remarks>
        /// Conversion of string to integer value is always defined, but in certain cases, we want to know
        /// if the conversion is successful (e.g. When creating a new array using index of string)
        /// </remarks>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">String to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, StringValue value,
            out IntegerValue convertedValue)
        {
            // TODO: Implement the correct convert to integer
            int result;
            if (int.TryParse(value.Value, out result))
            {
                convertedValue = outset.CreateInt(result);
                return true;
            }
            else
            {
                convertedValue = outset.CreateInt(0);
                return false;
            }
        }

        /// <summary>
        /// Determines value of integer from content of the array value.
        /// </summary>
        /// <remarks>
        /// Here the documentation is ambiguous. It says that the behavior of converting to integer
        /// is undefined for other than scalar types. However, it typically acts as predefined
        /// function <c>intval</c> which has conversion of to array defined clearly.
        /// </remarks>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Array to convert</param>
        /// <returns>1 if array has at least one element, otherwise 0</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, AssociativeArray value)
        {
            var indices = outset.IterateArray(value);
            var enumerator = indices.GetEnumerator();
            return outset.CreateInt(enumerator.MoveNext() ? 1 : 0);
        }

        /// <summary>
        /// Converts an undefined value to an equivalent integer value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Always 0 value</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateInt(0);
        }

        #endregion

        #region ToFloat

        /// <summary>
        /// Converts the boolean value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Boolean value to convert</param>
        /// <returns>The number 1.0 if value is <c>true</c>, otherwise 0.0</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, BooleanValue value)
        {
            return outset.CreateDouble(System.Convert.ToDouble(value.Value));
        }

        /// <summary>
        /// Converts the value of integer value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Integer value to convert</param>
        /// <returns>A floating-point number that is equivalent to integer value.</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, IntegerValue value)
        {
            return outset.CreateDouble(System.Convert.ToDouble(value.Value));
        }

        /// <summary>
        /// Converts the value of long integer value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Long integer value to convert</param>
        /// <returns>A floating-point number that is equivalent to long integer value.</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, LongintValue value)
        {
            return outset.CreateDouble(System.Convert.ToDouble(value.Value));
        }

        /// <summary>
        /// Converts the string value to corresponding floating-point number.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">String to convert</param>
        /// <returns>Number representation of string if it can be converted, otherwise 0.0</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, StringValue value)
        {
            // TODO: Implement the correct convert to float
            double result;
            if (double.TryParse(value.Value, out result))
            {
                return outset.CreateDouble(result);
            }
            else
            {
                return outset.CreateDouble(0.0);
            }
        }

        /// <summary>
        /// Determines floating-point number from content of the array value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.ToInteger(FlowOutputSet, AssociativeArray)"/>
        /// </remarks>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Array to convert</param>
        /// <returns>1.0 if array has at least one element, otherwise 0.0</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, AssociativeArray value)
        {
            var indices = outset.IterateArray(value);
            var enumerator = indices.GetEnumerator();
            return outset.CreateDouble(enumerator.MoveNext() ? 1.0 : 0.0);
        }

        /// <summary>
        /// Converts an undefined value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Always 0.0 value</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateDouble(0.0);
        }

        #endregion

        #region ToString

        /// <summary>
        /// Converts the boolean value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Boolean value to convert</param>
        /// <returns>String "1" if value is <c>true</c>, otherwise empty string</returns>
        public static StringValue ToString(FlowOutputSet outset, BooleanValue value)
        {
            return outset.CreateString(value.Value ? "1" : string.Empty);
        }

        /// <summary>
        /// Converts the integer value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Value of integer to convert</param>
        /// <returns>The string representation of integer value</returns>
        public static StringValue ToString(FlowOutputSet outset, IntegerValue value)
        {
            return outset.CreateString(System.Convert.ToString(value.Value));
        }

        /// <summary>
        /// Converts the long integer value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Value of long integer to convert</param>
        /// <returns>The string representation of long integer value</returns>
        public static StringValue ToString(FlowOutputSet outset, LongintValue value)
        {
            return outset.CreateString(System.Convert.ToString(value.Value));
        }

        /// <summary>
        /// Converts the floating-point number to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <returns>The string representation of floating-point number</returns>
        public static StringValue ToString(FlowOutputSet outset, FloatValue value)
        {
            return outset.CreateString(System.Convert.ToString(value.Value));
        }

        /// <summary>
        /// Determines string value from content of the array value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Array to convert</param>
        /// <returns>Always "Array" string</returns>
        public static StringValue ToString(FlowOutputSet outset, AssociativeArray value)
        {
            return outset.CreateString("Array");
        }

        /// <summary>
        /// Determines string value from the reference to external resource.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">External resource to convert</param>
        /// <returns>
        /// Value "Resource id #X", where X is a unique number assigned to the resource by PHP at runtime
        /// </returns>
        public static StringValue ToString(FlowOutputSet outset, ResourceValue value)
        {
            return outset.CreateString(string.Concat("Resource id #", value.UID.ToString()));
        }

        /// <summary>
        /// Determines string value from content of any array value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Any array to convert</param>
        /// <returns>Always "Array" string</returns>
        public static StringValue ToString(FlowOutputSet outset, AnyArrayValue value)
        {
            return outset.CreateString("Array");
        }

        /// <summary>
        /// Converts an undefined value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Empty string</returns>
        public static StringValue ToString(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateString(string.Empty);
        }

        #endregion

        #region ToObject

        /// <summary>
        /// Converts the array to corresponding new object.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Array to convert</param>
        /// <returns>Object with fields named by indices of array and initialized by their values</returns>
        public static ObjectValue ToObject(FlowOutputSet outset, AssociativeArray value)
        {
            var objectValue = CreateStandardObject(outset);
            var indices = outset.IterateObject(objectValue);

            foreach (var index in indices)
            {
                var entry = outset.GetIndex(value, index);
                outset.SetField(objectValue, index, entry);
            }

            return objectValue;
        }

        /// <summary>
        /// Creates an object containing one field with the undefined value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Object with field named "scalar" which contains undefined value</returns>
        public static ObjectValue ToObject(FlowOutputSet outset, UndefinedValue value)
        {
            return CreateStandardObject(outset);
        }

        /// <summary>
        /// Creates an object containing one field with a value but an object, array or undefined value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">A value to convert</param>
        /// <returns>Object with field named "scalar" which contains the value</returns>
        public static ObjectValue ToObject(FlowOutputSet outset, Value value)
        {
            var objectValue = CreateStandardObject(outset);
            var index = outset.CreateIndex("scalar");
            outset.SetField(objectValue, index, new MemoryEntry(value));
            return objectValue;
        }

        #endregion

        #region ToArray

        /// <summary>
        /// Converts the object to corresponding array structure.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Object of any type to convert</param>
        /// <returns>Array with keys named by fields of object and initialized by their values</returns>
        public static AssociativeArray ToArray(FlowOutputSet outset, ObjectValue value)
        {
            // TODO: This conversion is quite difficult. It does not convert integer properties, needs to
            // know visibility of every property and needs access to private properties of base classes.

            var array = outset.CreateArray();
            var indices = outset.IterateObject(value);

            foreach (var index in indices)
            {
                var field = outset.GetField(value, index);
                outset.SetIndex(array, index, field);
            }

            return array;
        }

        /// <summary>
        /// Converts an undefined value to corresponding array structure.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Empty with no elements</returns>
        public static AssociativeArray ToArray(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateArray();
        }

        /// <summary>
        /// Converts a value but an object, array or undefined value to corresponding array structure.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">A value to convert</param>
        /// <returns>Array with a single element with the value on position of 0 index</returns>
        public static AssociativeArray ToArray(FlowOutputSet outset, Value value)
        {
            var array = outset.CreateArray();
            var index = outset.CreateIndex("0");
            outset.SetIndex(array, index, new MemoryEntry(value));
            return array;
        }

        #endregion

        #region ToIntegerInterval

        /// <summary>
        /// Tries to convert the interval of long integer to an equivalent integer interval.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Long integer to convert</param>
        /// <param name="convertedValue">
        /// Integer interval in the same range as input if conversion is successful, otherwise (0;0)
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToIntegerInterval(FlowOutputSet outset, LongintIntervalValue value,
            out IntegerIntervalValue convertedValue)
        {
            int castedStart, castedEnd = 0;
            var isConverted = TryConvertToInteger(outset, value.Start, out castedStart)
                && TryConvertToInteger(outset, value.End, out castedEnd);
            convertedValue = outset.CreateIntegerInterval(castedStart, castedEnd);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the interval of floating-point numbers to an equivalent integer interval.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(FlowOutputSet, double, out int)"/>
        /// </remarks>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <param name="convertedValue">
        /// Integer interval in the same range as input if conversion is successful, otherwise (0;0)
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToIntegerInterval(FlowOutputSet outset, FloatIntervalValue value,
            out IntegerIntervalValue convertedValue)
        {
            int castedStart, castedEnd = 0;
            var isConverted = TryConvertToInteger(outset, value.Start, out castedStart)
                && TryConvertToInteger(outset, value.End, out castedEnd);
            convertedValue = outset.CreateIntegerInterval(castedStart, castedEnd);
            return isConverted;
        }

        #endregion

        #region Conversion between natve types

        /// <summary>
        /// Tries to convert the value of long integer to an equivalent integer value.
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Native long integer to convert</param>
        /// <param name="convertedValue">Integer type value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, long value,
            out int convertedValue)
        {
            // This condition suppresses <c>OverflowException</c> of <c>Convert.ToInt32</c> conversion.
            if (value < int.MinValue || value > int.MaxValue)
            {
                convertedValue = 0;
                return false;
            }

            convertedValue = System.Convert.ToInt32(value);
            return true;
        }

        /// <summary>
        /// Tries to convert the value of floating-point number to an equivalent integer value.
        /// </summary>
        /// <remarks>
        /// In PHP 5, when converting from floating-point number to integer, the number is rounded
        /// towards zero. If the number is beyond the boundaries of integer, the result is undefined
        /// integer. No warning, not even a notice will be issued when this happens.
        /// </remarks>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <param name="value">Native floating-point number to convert</param>
        /// <param name="convertedValue">Integer type value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        private static bool TryConvertToInteger(FlowOutputSet outset, double value,
            out int convertedValue)
        {
            var truncated = Math.Truncate(value);

            // This condition suppresses <c>OverflowException</c> of <c>Convert.ToInt32</c> conversion.
            if (double.IsNaN(truncated) || truncated < int.MinValue || truncated > int.MaxValue)
            {
                convertedValue = 0;
                return false;
            }

            convertedValue = System.Convert.ToInt32(truncated);
            return true;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Creates a new object of build-in type "stdClass"
        /// </summary>
        /// <param name="outset">Output set of FlowInfo</param>
        /// <returns>Object of "stdClass" type with no fields nor methods</returns>
        private static ObjectValue CreateStandardObject(FlowOutputSet outset)
        {
            var standardClass = outset.ResolveType(new QualifiedName(new Name("stdClass")));
            var enumerator = standardClass.GetEnumerator();
            enumerator.MoveNext();
            return outset.CreateObject(enumerator.Current as TypeValue);
        }

        #endregion
    }

    public class ValueTypeResolver
    {
        public static bool isInt(Value value)
        {
            return value is IntegerIntervalValue || value is IntegerValue || value is AnyIntegerValue;
        }

        public static bool isLong(Value value)
        {
            return value is LongintValue || value is AnyLongintValue || value is LongintIntervalValue;
        }

        public static bool isFloat(Value value)
        {
            return value is FloatIntervalValue || value is FloatValue || value is AnyFloatValue;
        }

        public static bool isBool(Value value)
        {
            return value is BooleanValue || value is AnyBooleanValue;
        }

        public static bool isString(Value value)
        {
            return value is StringValue || value is AnyStringValue;
        }

        public static bool isObject(Value value)
        {
            return value is ObjectValue || value is AnyObjectValue;
        }

        public static bool isArray(Value value)
        {
            return value is AssociativeArray || value is AnyArrayValue;
        }

        public static bool CanBeDirty(Value value)
        {
            return !(ValueTypeResolver.isBool(value)
                || ValueTypeResolver.isInt(value)
                || ValueTypeResolver.isFloat(value)
                || ValueTypeResolver.isLong(value));
        }
    }
}
