using System;
using System.Globalization;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts values of various PHP data types to another types.
    /// </summary>
    /// <remarks>
    /// The class <see cref="TypeConversion" /> with its static methods serves as converter between native
    /// and even user-defined (in case of objects) PHP types. The class is very similar to
    /// <c>System.Convert</c> class in .NET Framework. It is highly recommended to prefer this class to .NET
    /// one even if the conversions between equivalent PHP and .NET types do not differ, because it is more
    /// expressive and not so error prone. All these types are supported: Boolean, integer, floating point
    /// number, string, array, object, resource and NULL value (the only value of null type). However,
    /// conversion between every two types is not supported. There are particular cases that may occur:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// There is no conversion. This is the case of the conversion between the same types.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Conversion is not defined. There are conversions that return the right type, but does not make
    /// any sense (e.g. conversion of object to integer). The result of operation is implementation-defined
    /// and analysis should return an abstract interpretation of the given type.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Conversion can fail. Some conversion depends on particular value and in some cases can fail
    /// (e.g. conversion of too large floating point number to integer). For that reason, there are methods
    /// that try to perform conversion and indicate whether they succeed.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A successful conversion. All other conversions will succeed even if new value result lose some data.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class TypeConversion
    {
        /// <summary>
        /// Name of standard generic empty class used for typecasting to object
        /// </summary>
        private static readonly QualifiedName standardClass = new QualifiedName(new Name("stdClass"));

        #region ToBoolean

        /// <summary>
        /// Converts the numeric value to an equivalent boolean value.
        /// </summary>
        /// <typeparam name="T">Type of number representation</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Numeric value to convert</param>
        /// <returns><c>true</c> if number is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean<T>(FlowOutputSet outset, NumericValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return outset.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Converts the native numeric value to an equivalent native boolean value.
        /// </summary>
        /// <typeparam name="T">Type of number representation</typeparam>
        /// <param name="value">Native numeric value to convert</param>
        /// <returns><c>true</c> if native number is not zero, otherwise <c>false</c></returns>
        public static bool ToBoolean<T>(NumericValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return !value.Value.Equals(value.Zero);
        }

        /// <summary>
        /// Converts the value of integer to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Integer to convert</param>
        /// <returns><c>true</c> if integer value is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, IntegerValue value)
        {
            return outset.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the value of native integer to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Native integer to convert</param>
        /// <returns><c>true</c> if integer value is not zero, otherwise <c>false</c></returns>
        public static bool ToBoolean(int value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts the value of long integer to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Long integer to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, LongintValue value)
        {
            return outset.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the value of native long integer to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Native long integer to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static bool ToBoolean(long value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts the value of floating-point number to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, FloatValue value)
        {
            return outset.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the value of native floating-point number to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Native floating-point number to convert</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c></returns>
        public static bool ToBoolean(double value)
        {
            return value != 0.0;
        }

        /// <summary>
        /// Converts the string value to proper boolean value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">String to convert</param>
        /// <returns><c>true</c> if string is not empty or "0", otherwise <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, StringValue value)
        {
            return outset.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the native string value to proper native boolean value.
        /// </summary>
        /// <param name="value">Native string to convert</param>
        /// <returns><c>true</c> if string is not empty or "0", otherwise <c>false</c></returns>
        public static bool ToBoolean(string value)
        {
            Debug.Assert(value != null, "String converted to boolean can never be null");

            return (value.Length != 0) && (!string.Equals(value, "0"));
        }

        /// <summary>
        /// Determines boolean value from the object reference value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Object of any type to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, ObjectValue value)
        {
            return outset.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from the object reference value.
        /// </summary>
        /// <param name="value">Object of any type to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static bool ToBoolean(ObjectValue value)
        {
            // Notice that in PHP 4, an object evaluates as false if it has no properties.
            return true;
        }

        /// <summary>
        /// Determines boolean value from content of the array value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
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
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">External resource to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, ResourceValue value)
        {
            return outset.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from the reference to external resource.
        /// </summary>
        /// <param name="value">External resource to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static bool ToBoolean(ResourceValue value)
        {
            return true;
        }

        /// <summary>
        /// Determines boolean value from any object reference value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Any object of any type to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, AnyObjectValue value)
        {
            return outset.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from any object reference value.
        /// </summary>
        /// <param name="value">Any object of any type to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static bool ToBoolean(AnyObjectValue value)
        {
            // Notice that in PHP 4, an object evaluates as false if it has no properties.
            return true;
        }

        /// <summary>
        /// Determines boolean value from any reference to external resource.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Any external resource to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, AnyResourceValue value)
        {
            return outset.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from any reference to external resource.
        /// </summary>
        /// <param name="value">Any external resource to convert</param>
        /// <returns>Always <c>true</c></returns>
        public static bool ToBoolean(AnyResourceValue value)
        {
            return true;
        }

        /// <summary>
        /// Converts possible interval of numbers to an equivalent concrete or abstract boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval</typeparam>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Value representing interval of numbers to convert</param>
        /// <returns>Concrete boolean value if it is possible, otherwise abstract boolean value</returns>
        public static Value ToBoolean<T>(FlowOutputSet outset, IntervalValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((value.Start.CompareTo(value.Zero) <= 0) && (value.End.CompareTo(value.Zero) >= 0))
            {
                if (value.Start.Equals(value.Zero) && value.End.Equals(value.Zero))
                {
                    return outset.CreateBool(false);
                }
                else
                {
                    return outset.AnyBooleanValue;
                }
            }
            else
            {
                return outset.CreateBool(true);
            }
        }

        /// <summary>
        /// Tries to convert possible interval of numbers to an equivalent boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval</typeparam>
        /// <param name="outset">Output set of a program point</param>
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
            bool casted;
            var isConverted = TryConvertToBoolean(value, out casted);
            convertedValue = outset.CreateBool(casted);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert possible interval of numbers to an equivalent native boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval</typeparam>
        /// <param name="value">Value representing interval of numbers to convert</param>
        /// <param name="convertedValue">
        /// <c>true</c> if interval does not contain zero and
        /// <c>false</c> if interval consists only from zero value or other cases.
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToBoolean<T>(IntervalValue<T> value, out bool convertedValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((value.Start.CompareTo(value.Zero) <= 0) && (value.End.CompareTo(value.Zero) >= 0))
            {
                if (value.Start.Equals(value.Zero) && value.End.Equals(value.Zero))
                {
                    convertedValue = false;
                    return true;
                }
                else
                {
                    convertedValue = false;
                    return false;
                }
            }
            else
            {
                convertedValue = true;
                return true;
            }
        }

        /// <summary>
        /// Converts an undefined value to an equivalent boolean value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Always <c>false</c></returns>
        public static BooleanValue ToBoolean(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Undefined value</param>
        /// <returns>Always <c>false</c></returns>
        public static bool ToBoolean(UndefinedValue value)
        {
            return false;
        }

        #endregion ToBoolean

        #region ToInteger

        /// <summary>
        /// Converts the boolean value to an equivalent value of integer.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Boolean value to convert</param>
        /// <returns>The number 1 if value is <c>true</c>, otherwise 0</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, BooleanValue value)
        {
            return outset.CreateInt(ToInteger(value.Value));
        }

        /// <summary>
        /// Converts the native boolean value to an equivalent value of native integer.
        /// </summary>
        /// <param name="value">Native boolean value to convert</param>
        /// <returns>The number 1 if value is <c>true</c>, otherwise 0</returns>
        public static int ToInteger(bool value)
        {
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tries to convert the value of long integer to an equivalent integer value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Long integer to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, LongintValue value,
            out IntegerValue convertedValue)
        {
            int casted;
            var isConverted = TryConvertToInteger(value.Value, out casted);
            convertedValue = outset.CreateInt(casted);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the value of native long integer to an equivalent native integer value.
        /// </summary>
        /// <param name="value">Native long integer to convert</param>
        /// <param name="convertedValue">Integer type value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(long value, out int convertedValue)
        {
            // This condition suppresses <c>OverflowException</c> of <c>Convert.ToInt32</c> conversion.
            if (value < int.MinValue || value > int.MaxValue)
            {
                convertedValue = 0;
                return false;
            }

            convertedValue = System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>
        /// Tries to convert the value of floating-point number to an equivalent integer value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(double, out int)" />
        /// </remarks>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, FloatValue value,
            out IntegerValue convertedValue)
        {
            int casted;
            var isConverted = TryConvertToInteger(value.Value, out casted);
            convertedValue = outset.CreateInt(casted);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the value of native floating-point number to an equivalent native integer value.
        /// </summary>
        /// <remarks>
        /// In PHP 5, when converting from floating-point number to integer, the number is rounded
        /// towards zero. If the number is beyond the boundaries of integer, the result is undefined
        /// integer. No warning, not even a notice will be issued when this happens.
        /// </remarks>
        /// <param name="value">Native floating-point number to convert</param>
        /// <param name="convertedValue">Integer type value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(double value, out int convertedValue)
        {
            var truncated = Math.Truncate(value);

            // This condition suppresses <c>OverflowException</c> of <c>Convert.ToInt32</c> conversion.
            if (double.IsNaN(truncated) || truncated < int.MinValue || truncated > int.MaxValue)
            {
                convertedValue = 0;
                return false;
            }

            convertedValue = System.Convert.ToInt32(truncated, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>
        /// Converts the string value to corresponding integer value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">String to convert</param>
        /// <returns>Integer representation of string if it can be converted, otherwise 0</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, StringValue value)
        {
            IntegerValue convertedValue;
            TryConvertToInteger(outset, value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Converts the native string value to corresponding native integer value.
        /// </summary>
        /// <param name="value">Native string to convert</param>
        /// <returns>Integer representation of string if it can be converted, otherwise 0</returns>
        public static int ToInteger(string value)
        {
            int convertedValue;
            TryConvertToInteger(value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Tries to convert the string value to corresponding integer value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(string, out int)" />
        /// </remarks>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">String to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(FlowOutputSet outset, StringValue value,
            out IntegerValue convertedValue)
        {
            int integerValue;
            var isSuccessful = TryConvertToInteger(value.Value, out integerValue);
            convertedValue = outset.CreateInt(integerValue);
            return isSuccessful;
        }

        /// <summary>
        /// Tries to convert the native string value to corresponding native integer value.
        /// </summary>
        /// <remarks>
        /// Conversion of string to integer value is always defined, but in certain cases, we want to know
        /// if the conversion is successful (e.g. explicit type-casting or when creating a new array using
        /// index of string) In these cases, hexadecimal numbers are not recognized
        /// </remarks>
        /// <param name="value">Native string to convert</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(string value, out int convertedValue)
        {
            double floatValue;
            bool isInteger;
            var isSuccessful = TryConvertToNumber(value, false, out convertedValue,
                out floatValue, out isInteger);
            return isSuccessful && isInteger;
        }

        /// <summary>
        /// Determines value of integer from content of the array value.
        /// </summary>
        /// <remarks>
        /// Here the documentation is ambiguous. It says that the behavior of converting to integer
        /// is undefined for other than scalar types. However, it typically acts as predefined
        /// function <c>intval</c> which has conversion of to array defined clearly.
        /// </remarks>
        /// <param name="outset">Output set of a program point</param>
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
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Always 0 value</returns>
        public static IntegerValue ToInteger(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateInt(ToInteger(value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent native integer value.
        /// </summary>
        /// <param name="value">Undefined value</param>
        /// <returns>Always 0 value</returns>
        public static int ToInteger(UndefinedValue value)
        {
            return 0;
        }

        #endregion ToInteger

        #region ToFloat

        /// <summary>
        /// Converts the boolean value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Boolean value to convert</param>
        /// <returns>The number 1.0 if value is <c>true</c>, otherwise 0.0</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, BooleanValue value)
        {
            return outset.CreateDouble(TypeConversion.ToFloat(value.Value));
        }

        /// <summary>
        /// Converts the native boolean value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Native boolean value to convert</param>
        /// <returns>The number 1.0 if value is <c>true</c>, otherwise 0.0</returns>
        public static double ToFloat(bool value)
        {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the value of integer value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Integer value to convert</param>
        /// <returns>A floating-point number that is equivalent to integer value.</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, IntegerValue value)
        {
            return outset.CreateDouble(TypeConversion.ToFloat(value.Value));
        }

        /// <summary>
        /// Converts the value of native integer value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Native integer value to convert</param>
        /// <returns>A floating-point number that is equivalent to native integer value.</returns>
        public static double ToFloat(int value)
        {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the value of long integer value to an equivalent floating-point number.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Long integer value to convert</param>
        /// <returns>A floating-point number that is equivalent to long integer value.</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, LongintValue value)
        {
            return outset.CreateDouble(TypeConversion.ToFloat(value.Value));
        }

        /// <summary>
        /// Converts the value of native long integer value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Native long integer value to convert</param>
        /// <returns>A floating-point number that is equivalent to native long integer value.</returns>
        public static double ToFloat(long value)
        {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the string value to corresponding floating-point number.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">String to convert</param>
        /// <returns>Number representation of string if it can be converted, otherwise 0.0</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, StringValue value)
        {
            FloatValue convertedValue;
            TryConvertToFloat(outset, value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Tries to convert the string value to corresponding floating-point number.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(string, out double)" />
        /// </remarks>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">String to convert</param>
        /// <param name="convertedValue">Converted value if conversion is successful, otherwise 0.0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToFloat(FlowOutputSet outset, StringValue value,
            out FloatValue convertedValue)
        {
            double floatValue;
            var isSuccessful = TryConvertToFloat(value.Value, out floatValue);
            convertedValue = outset.CreateDouble(floatValue);
            return isSuccessful;
        }

        /// <summary>
        /// Tries to convert the native string value to corresponding native floating-point number.
        /// </summary>
        /// <remarks>
        /// Conversion of string to floating-point number is always defined, but in certain cases,
        /// we want to know if the conversion is successful (e.g. explicit type-casting)
        /// </remarks>
        /// <param name="value">Native string to convert</param>
        /// <param name="convertedValue">Converted value if conversion is successful, otherwise 0.0</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToFloat(string value, out double convertedValue)
        {
            int integerValue;
            bool isInteger;
            var isSuccessful = TryConvertToNumber(value, false, out integerValue,
                out convertedValue, out isInteger);
            return isSuccessful || (!isInteger);
        }

        /// <summary>
        /// Determines floating-point number from content of the array value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.ToInteger(FlowOutputSet, AssociativeArray)" />
        /// </remarks>
        /// <param name="outset">Output set of a program point</param>
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
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Always 0.0 value</returns>
        public static FloatValue ToFloat(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateDouble(0.0);
        }

        #endregion ToFloat

        #region ToString

        /// <summary>
        /// Converts the boolean value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Boolean value to convert</param>
        /// <returns>String "1" if value is <c>true</c>, otherwise empty string</returns>
        public static StringValue ToString(FlowOutputSet outset, BooleanValue value)
        {
            return outset.CreateString(ToString(value.Value));
        }

        /// <summary>
        /// Converts the native boolean value to an equivalent native string representation.
        /// </summary>
        /// <param name="value">Native boolean value to convert</param>
        /// <returns>String "1" if value is <c>true</c>, otherwise empty string</returns>
        public static string ToString(bool value)
        {
            return value ? "1" : string.Empty;
        }

        /// <summary>
        /// Converts the integer value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Value of integer to convert</param>
        /// <returns>The string representation of integer value</returns>
        public static StringValue ToString(FlowOutputSet outset, IntegerValue value)
        {
            return outset.CreateString(ToString(value.Value));
        }

        /// <summary>
        /// Converts the native integer value to an equivalent native string representation.
        /// </summary>
        /// <param name="value">Value of native integer to convert</param>
        /// <returns>The string representation of native integer value</returns>
        public static string ToString(int value)
        {
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the long integer value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Value of long integer to convert</param>
        /// <returns>The string representation of long integer value</returns>
        public static StringValue ToString(FlowOutputSet outset, LongintValue value)
        {
            return outset.CreateString(System.Convert.ToString(value.Value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Converts the floating-point number to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <returns>The string representation of floating-point number</returns>
        public static StringValue ToString(FlowOutputSet outset, FloatValue value)
        {
            return outset.CreateString(ToString(value.Value));
        }

        /// <summary>
        /// Converts the native floating-point number to an equivalent native string representation.
        /// </summary>
        /// <param name="value">Native floating-point number to convert</param>
        /// <returns>The string representation of native floating-point number</returns>
        public static string ToString(double value)
        {
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines string value from content of the array value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Array to convert</param>
        /// <returns>Always "Array" string</returns>
        public static StringValue ToString(FlowOutputSet outset, AssociativeArray value)
        {
            return outset.CreateString("Array");
        }

        /// <summary>
        /// Determines string value from the reference to external resource.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">External resource to convert</param>
        /// <returns>
        /// Value "Resource id #X", where X is a unique number assigned to the resource by PHP at runtime
        /// </returns>
        public static StringValue ToString(FlowOutputSet outset, ResourceValue value)
        {
            return outset.CreateString(string.Concat("Resource id #",
                value.UID.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Determines string value from content of any array value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Any array to convert</param>
        /// <returns>Always "Array" string</returns>
        public static StringValue ToString(FlowOutputSet outset, AnyArrayValue value)
        {
            return outset.CreateString("Array");
        }

        /// <summary>
        /// Converts an undefined value to an equivalent string representation.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Empty string</returns>
        public static StringValue ToString(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateString(string.Empty);
        }

        #endregion ToString

        #region ToObject

        /// <summary>
        /// Converts the array to corresponding new object.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Array to convert</param>
        /// <returns>Object with fields named by indices of array and initialized by their values</returns>
        public static ObjectValue ToObject(FlowOutputSet outset, AssociativeArray value)
        {
            var objectValue = CreateStandardObject(outset);
            var objectEntry = GetSnapshotEntry(outset, objectValue);
            var arrayEntry = GetSnapshotEntry(outset, value);

            var outSnapshot = outset.Snapshot;

            var indices = outset.IterateArray(value);

            foreach (var index in indices)
            {
                var fieldEntry = GetFieldEntry(outSnapshot, objectEntry, index.Identifier);
                var readValue = fieldEntry.ReadMemory(outSnapshot);

                var indexEntry = GetIndexEntry(outSnapshot, arrayEntry, index.Identifier);
                indexEntry.WriteMemory(outSnapshot, readValue);
            }

            return objectValue;
        }

        /// <summary>
        /// Creates an object containing one field with the undefined value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Object with field named "scalar" which contains undefined value</returns>
        public static ObjectValue ToObject(FlowOutputSet outset, UndefinedValue value)
        {
            return CreateStandardObject(outset);
        }

        /// <summary>
        /// Creates an object containing one field with a value but an object, array or undefined value.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">A value to convert</param>
        /// <returns>Object with field named "scalar" which contains the value</returns>
        public static ObjectValue ToObject(FlowOutputSet outset, Value value)
        {
            var objectValue = CreateStandardObject(outset);
            var objectEntry = GetSnapshotEntry(outset, objectValue);

            var outSnapshot = outset.Snapshot;
            var fieldEntry = GetFieldEntry(outSnapshot, objectEntry, "scalar");

            var valueEntry = new MemoryEntry(value);
            fieldEntry.WriteMemory(outSnapshot, valueEntry);

            return objectValue;
        }

        #endregion ToObject

        #region ToArray

        /// <summary>
        /// Converts the object to corresponding array structure.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Object of any type to convert</param>
        /// <returns>Array with keys named by fields of object and initialized by their values</returns>
        public static AssociativeArray ToArray(FlowOutputSet outset, ObjectValue value)
        {
            // TODO: This conversion is quite difficult. It does not convert integer properties, needs to
            // know visibility of every property and needs access to private properties of base classes.

            var arrayValue = outset.CreateArray();
            var arrayEntry = GetSnapshotEntry(outset, arrayValue);
            var objectEntry = GetSnapshotEntry(outset, value);

            var outSnapshot = outset.Snapshot;

            var indices = outset.IterateObject(value);

            foreach (var index in indices)
            {
                var indexEntry = GetIndexEntry(outSnapshot, arrayEntry, index.Identifier);
                var readValue = indexEntry.ReadMemory(outSnapshot);

                var fieldEntry = GetFieldEntry(outSnapshot, objectEntry, index.Identifier);
                fieldEntry.WriteMemory(outSnapshot, readValue);
            }

            return arrayValue;
        }

        /// <summary>
        /// Converts an undefined value to corresponding array structure.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Undefined value</param>
        /// <returns>Empty with no elements</returns>
        public static AssociativeArray ToArray(FlowOutputSet outset, UndefinedValue value)
        {
            return outset.CreateArray();
        }

        /// <summary>
        /// Converts a value but an object, array or undefined value to corresponding array structure.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">A value to convert</param>
        /// <returns>Array with a single element with the value on position of 0 index</returns>
        public static AssociativeArray ToArray(FlowOutputSet outset, Value value)
        {
            var arrayValue = outset.CreateArray();
            var arrayEntry = GetSnapshotEntry(outset, arrayValue);

            var outSnapshot = outset.Snapshot;
            var indexEntry = GetIndexEntry(outSnapshot, arrayEntry, "0");

            var valueEntry = new MemoryEntry(value);
            indexEntry.WriteMemory(outSnapshot, valueEntry);

            return arrayValue;
        }

        #endregion ToArray

        #region ToIntegerInterval

        /// <summary>
        /// Tries to convert the interval of long integer to an equivalent integer interval.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Long integer to convert</param>
        /// <param name="convertedValue">
        /// Integer interval in the same range as input if conversion is successful, otherwise (0;0)
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToIntegerInterval(FlowOutputSet outset, LongintIntervalValue value,
            out IntegerIntervalValue convertedValue)
        {
            int castedStart, castedEnd = 0;
            var isConverted = TryConvertToInteger(value.Start, out castedStart)
                && TryConvertToInteger(value.End, out castedEnd);
            convertedValue = outset.CreateIntegerInterval(castedStart, castedEnd);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the interval of floating-point numbers to an equivalent integer interval.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(FlowOutputSet, double, out int)" />
        /// </remarks>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Floating-point number to convert</param>
        /// <param name="convertedValue">
        /// Integer interval in the same range as input if conversion is successful, otherwise (0;0)
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c></returns>
        public static bool TryConvertToIntegerInterval(FlowOutputSet outset, FloatIntervalValue value,
            out IntegerIntervalValue convertedValue)
        {
            int castedStart, castedEnd = 0;
            var isConverted = TryConvertToInteger(value.Start, out castedStart)
                && TryConvertToInteger(value.End, out castedEnd);
            convertedValue = outset.CreateIntegerInterval(castedStart, castedEnd);
            return isConverted;
        }

        #endregion ToIntegerInterval

        #region ToFloatInterval

        /// <summary>
        /// Extends integer interval into floating-point interval of the same range.
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <param name="value">Integer interval to extend</param>
        /// <returns>Floating-point interval extended from integer interval</returns>
        public static FloatIntervalValue ToFloatInterval(FlowOutputSet outset, IntegerIntervalValue value)
        {
            return outset.CreateFloatInterval(value.Start, value.End);
        }

        #endregion ToFloatInterval

        #region ToNumber

        /// <summary>
        /// Tries to convert the string value to integer value and if it fails, then to floating-point number
        /// </summary>
        /// <remarks>
        /// Conversion to number distinguishes if value is integer or floating-point constant and creates
        /// integer whenever it is possible. A valid number constant is represented by regular expression
        /// "[:space:]*(0[xX][0-9a-fA-F]+|[+-]?[0-9]*([0-9]([\.][0-9]*)?|[\.][0-9]+)([eE][+-]?[0-9]+)?)".
        /// Conversion does not work the same as PHP scanner. In the first place, it absolutely ignores
        /// binary and octane numbers. On the contrary, it permits whitespaces at the beginning and tolerates
        /// characters after valid number format. In other words, it tries to parse everything what is
        /// possible. In this respect it behaves identically to C function <c>strtod</c>. Finally, it is
        /// very odd how numbers are converted during different operations. If string is converted by
        /// an arithmetic or comparison operation, conversion is proper as described. However, if we convert
        /// explicitly by type-casting or with bitwise operation, hexadecimal numbers are not recognized.
        /// </remarks>
        /// <param name="value">String value to convert</param>
        /// <param name="canBeHexadecimal">Determines whether to parse hexadecimal format too</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0</param>
        /// <param name="floatValue">
        /// New floating-point number if conversion is successful or integer is too large, otherwise 0.0
        /// </param>
        /// <param name="isInteger">
        /// <c>true</c> if value is not converted to floating-point number, otherwise <c>false</c>
        /// </param>
        /// <param name="isHexadecimal">
        /// <c>true</c> if number is converted from string in hexadecimal format, otherwise <c>false</c>
        /// </param>
        /// <returns>
        /// <c>true</c> if value is converted successfully to integer or floating-point number, otherwise
        /// <c>false</c>, even if conversion to integer fails and result is stored as floating-point value
        /// </returns>
        public static bool TryConvertToNumber(string value, bool canBeHexadecimal, out int integerValue,
            out double floatValue, out bool isInteger, out bool isHexadecimal)
        {
            // Skip whitespaces at the beginning of the string
            var index = SkipWhiteSpace(value);

            if (canBeHexadecimal && (value.Length > index + 2) && (value[index] == '0')
                && ((value[index + 1] == 'x') || (value[index + 1] == 'X')))
            {
                index += 2;
                if (TryConvertHexadecialToInteger(value[index], out integerValue))
                {
                    // The hexadecimal format is converted to integer or float
                    isInteger = TryParseHexadecimal(value, index + 1, ref integerValue, out floatValue);
                    isHexadecimal = true;
                    return isInteger;
                }
                else
                {
                    // Conversion is valid because of the first zero
                    integerValue = 0;
                    floatValue = 0.0;
                    isInteger = true;
                    isHexadecimal = false;
                    return true;
                }
            }

            isHexadecimal = false;
            var start = index;
            index = SkipSign(value, index);

            // Skip digits in integer part
            var startDigits = index;
            index = SkipDigits(value, index);
            var isIntegerPart = index > startDigits;

            bool isFractionalPart;
            if (index >= value.Length)
            {
                if (isIntegerPart)
                {
                    // There is only integer part
                    isInteger = TryParseToInteger(value, start, value.Length - start,
                        out integerValue, out floatValue);
                    return isInteger;
                }
                else
                {
                    // There is the end before begin of a number
                    return SetParsingFailure(out integerValue, out floatValue, out isInteger);
                }
            }
            else if (value[index] == '.')
            {
                ++index;
                startDigits = index;
                index = SkipDigits(value, index);

                if ((index > startDigits) || isIntegerPart)
                {
                    // It is floating-point number, becasue there is decimal point
                    isFractionalPart = true;
                }
                else
                {
                    // Before and after decimal point is not any digit
                    return SetParsingFailure(out integerValue, out floatValue, out isInteger);
                }
            }
            else
            {
                if (isIntegerPart)
                {
                    // It is valid number without decimal point (i.e. still integer)
                    isFractionalPart = false;
                }
                else
                {
                    // Invalid character at the begin of the string
                    return SetParsingFailure(out integerValue, out floatValue, out isInteger);
                }
            }

            var end = SkipExponent(value, index);
            if (isFractionalPart || (end > index))
            {
                integerValue = 0;
                bool isSuccessful;

                // We identify a correct floating-point number format
                if ((start == 0) && (end == value.Length))
                {
                    isSuccessful = double.TryParse(value, out floatValue);
                }
                else
                {
                    isSuccessful = double.TryParse(value.Substring(start, end - start), out floatValue);
                }

                Debug.Assert(isSuccessful, "The string is definitely in floating-point number format");
                isInteger = false;
                return true;
            }
            else
            {
                // There is only integer part
                isInteger = TryParseToInteger(value, start, end - start, out integerValue, out floatValue);
                return isInteger;
            }
        }

        /// <summary>
        /// Tries to convert the string value to integer value and if it fails, then to floating-point number
        /// </summary>
        /// <param name="value">String value to convert</param>
        /// <param name="canBeHexadecimal">Determines whether to parse hexadecimal format too</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0</param>
        /// <param name="floatValue">
        /// New floating-point number if conversion is successful or integer is too large, otherwise 0.0
        /// </param>
        /// <param name="isInteger">
        /// <c>true</c> if value is not converted to floating-point number, otherwise <c>false</c>
        /// </param>
        /// <returns>
        /// <c>true</c> if value is converted successfully to integer or floating-point number, otherwise
        /// <c>false</c>, even if conversion to integer fails and result is stored as floating-point value
        /// </returns>
        public static bool TryConvertToNumber(string value, bool canBeHexadecimal, out int integerValue,
            out double floatValue, out bool isInteger)
        {
            bool isHexadecimal;
            return TryConvertToNumber(value, canBeHexadecimal, out integerValue, out floatValue,
                out isInteger, out isHexadecimal);
        }

        /// <summary>
        /// Tries to convert the string value to integer value and if it fails, then to floating-point number
        /// </summary>
        /// <param name="value">String in integer format value to parse</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0</param>
        /// <param name="floatValue">New floating-point number if conversion fails, otherwise 0.0</param>
        /// <returns><c>true</c> if value is parsed successfully, otherwise <c>false</c></returns>
        private static bool TryParseToInteger(string value, out int integerValue, out double floatValue)
        {
            Debug.Assert(value.Length > 0, "The string with number must not be empty");

            if (int.TryParse(value, out integerValue))
            {
                floatValue = integerValue;
                return true;
            }
            else
            {
                integerValue = 0;
                var isSuccessful = double.TryParse(value, out floatValue);
                Debug.Assert(isSuccessful, "The string is definitely in floating-point number format");
                return false;
            }
        }

        /// <summary>
        /// Tries to convert the substring to integer value and if it fails, then to floating-point number
        /// </summary>
        /// <param name="value">String value to parse</param>
        /// <param name="start">Start of the substring to parse</param>
        /// <param name="length">Length of the substring to parse</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0</param>
        /// <param name="floatValue">New floating-point number if conversion fails, otherwise 0.0</param>
        /// <returns><c>true</c> if value is parsed successfully, otherwise <c>false</c></returns>
        private static bool TryParseToInteger(string value, int start, int length,
            out int integerValue, out double floatValue)
        {
            Debug.Assert((start >= 0) && (length >= 0) && (length <= value.Length),
                "Start and length must indicate correct substring");

            if ((start == 0) && (length == value.Length))
            {
                return TryParseToInteger(value, out integerValue, out floatValue);
            }
            else
            {
                return TryParseToInteger(value.Substring(start, length), out integerValue, out floatValue);
            }
        }

        /// <summary>
        /// Tries to convert hexadecimal string to integer and if it fails, then to floating-point number
        /// </summary>
        /// <param name="value">String to convert, it must be in format "0x[0-9a-fA-F]+"</param>
        /// <param name="index">Position of the second digit of hexadecimal number within string</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0</param>
        /// <param name="floatValue">New floating-point number if conversion fails, otherwise 0.0</param>
        /// <returns><c>true</c> if value is parsed to integer successfully, otherwise <c>false</c></returns>
        private static bool TryParseHexadecimal(string value, int index, ref int integerValue,
            out double floatValue)
        {
            Debug.Assert((index > 2) && (value.Length >= index),
                "Index is the position of the second digit of hexadecimal number within string");

            long convertedLong = integerValue;
            for (; index < value.Length; ++index)
            {
                int hexaValue;
                if (TryConvertHexadecialToInteger(value[index], out hexaValue))
                {
                    convertedLong <<= 4;
                    convertedLong += hexaValue;
                }
                else
                {
                    break;
                }

                if (convertedLong > int.MaxValue)
                {
                    integerValue = 0;
                    floatValue = System.Convert.ToDouble(convertedLong, CultureInfo.InvariantCulture);

                    ++index;
                    for (; index < value.Length; ++index)
                    {
                        if (TryConvertHexadecialToInteger(value[index], out hexaValue))
                        {
                            floatValue *= 16;
                            floatValue += hexaValue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return false;
                }
            }

            integerValue = System.Convert.ToInt32(convertedLong, CultureInfo.InvariantCulture);
            floatValue = integerValue;
            return true;
        }

        /// <summary>
        /// Tries to convert character representing hexadecimal digit to integer value
        /// </summary>
        /// <param name="character">Character to convert</param>
        /// <param name="value">New integer value if conversion is successful, otherwise 0</param>
        /// <returns><c>true</c> if character is hexadecimal digit, otherwise <c>false</c></returns>
        private static bool TryConvertHexadecialToInteger(char character, out int value)
        {
            if (char.IsDigit(character))
            {
                value = character - '0';
                return true;
            }
            else if ((character >= 'a') && (character <= 'f'))
            {
                value = 10 + (character - 'a');
                return true;
            }
            else if ((character >= 'A') && (character <= 'F'))
            {
                value = 10 + (character - 'A');
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Finds out if the exponent part is at the beginning of string
        /// </summary>
        /// <param name="value">String value to search for</param>
        /// <param name="index">The starting character position to search for</param>
        /// <returns>
        /// The first position after exponent part if it is valid, otherwise <paramref name="index" />
        /// </returns>
        private static int SkipExponent(string value, int index)
        {
            if (index < value.Length)
            {
                var c = value[index];
                if ((c != 'e') && (c != 'E'))
                {
                    return index;
                }
            }
            else
            {
                return index;
            }

            var start = index;
            ++index;
            index = SkipSign(value, index);

            var end = SkipDigits(value, index);
            return (end > index) ? end : start;
        }

        /// <summary>
        /// Search the first non-digit character and returns its position
        /// </summary>
        /// <param name="value">String value to search for</param>
        /// <param name="index">The starting character position to search for</param>
        /// <returns>The first position of non-digit character or length of string</returns>
        private static int SkipDigits(string value, int index)
        {
            for (; index < value.Length; ++index)
            {
                if (!char.IsDigit(value[index]))
                {
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Determines if the string at given position is character + or - and if so, skips it
        /// </summary>
        /// <param name="value">String value to search for</param>
        /// <param name="index">The starting character position to search for</param>
        /// <returns>
        /// <paramref name="index" /> if character at the position is not + or -, otherwise the next position
        /// </returns>
        private static int SkipSign(string value, int index)
        {
            if (index < value.Length)
            {
                var c = value[index];
                if ((c == '+') || (c == '-'))
                {
                    return index + 1;
                }
            }

            return index;
        }

        /// <summary>
        /// Search the first non-whitespace character and returns its position
        /// </summary>
        /// <param name="value">String value to search for</param>
        /// <returns>The first position of non-whitespace character or length of string</returns>
        private static int SkipWhiteSpace(string value)
        {
            int index;
            for (index = 0; index < value.Length; ++index)
            {
                if (!char.IsWhiteSpace(value[index]))
                {
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Set all return values of <see cref="TypeConversion.TryConvertToNumber" /> to indicate failure
        /// </summary>
        /// <param name="integerValue">Converted integer value, always 0</param>
        /// <param name="floatValue">Converted floating-point value, always 0.0</param>
        /// <param name="isInteger">Always <c>true</c></param>
        /// <returns>Always <c>false</c></returns>
        private static bool SetParsingFailure(out int integerValue, out double floatValue, out bool isInteger)
        {
            integerValue = 0;
            floatValue = 0.0;
            isInteger = true;
            return false;
        }

        #endregion ToNumber

        #region Helper methods

        /// <summary>
        /// Creates a new object of build-in type <c>stdClass</c>
        /// </summary>
        /// <param name="outset">Output set of a program point</param>
        /// <returns>Object of <c>stdClass</c> type with no fields nor methods</returns>
        private static ObjectValue CreateStandardObject(FlowOutputSet outset)
        {
            var standardClassType = outset.ResolveType(standardClass);
            var enumerator = standardClassType.GetEnumerator();
            enumerator.MoveNext();
            return outset.CreateObject(enumerator.Current as TypeValue);
        }

        private static ReadSnapshotEntryBase GetSnapshotEntry(FlowOutputSet outset, Value value)
        {
            var entry = new MemoryEntry(value);
            return outset.CreateSnapshotEntry(entry);
        }

        private static ReadWriteSnapshotEntryBase GetFieldEntry(SnapshotBase snapshot,
            ReadSnapshotEntryBase objectEntry, string index)
        {
            var fieldIdentifier = new VariableIdentifier(index);
            return objectEntry.ReadField(snapshot, fieldIdentifier);
        }

        private static ReadWriteSnapshotEntryBase GetIndexEntry(SnapshotBase snapshot,
            ReadSnapshotEntryBase arrayEntry, string index)
        {
            var indexIdentifier = new MemberIdentifier(index);
            return arrayEntry.ReadIndex(snapshot, indexIdentifier);
        }

        #endregion Helper methods
    }

    public static class ValueTypeResolver
    {
        public static bool IsBool(Value value)
        {
            return value is BooleanValue || value is AnyBooleanValue;
        }

        public static bool IsInt(Value value)
        {
            return value is IntegerIntervalValue || value is IntegerValue || value is AnyIntegerValue;
        }

        public static bool IsLong(Value value)
        {
            return value is LongintValue || value is AnyLongintValue || value is LongintIntervalValue;
        }

        public static bool IsFloat(Value value)
        {
            return value is FloatIntervalValue || value is FloatValue || value is AnyFloatValue;
        }

        public static bool IsString(Value value)
        {
            return value is StringValue || value is AnyStringValue;
        }

        public static bool IsCompound(Value value)
        {
            return value is CompoundValue || value is AnyCompoundValue;
        }

        public static bool IsObject(Value value)
        {
            return value is ObjectValue || value is AnyObjectValue;
        }

        public static bool IsArray(Value value)
        {
            return value is AssociativeArray || value is AnyArrayValue;
        }

        public static bool IsResource(Value value)
        {
            return value is ResourceValue || value is AnyResourceValue;
        }

        public static bool CanBeDirty(Value value)
        {
            return !(ValueTypeResolver.IsBool(value)
                || ValueTypeResolver.IsInt(value)
                || ValueTypeResolver.IsFloat(value)
                || ValueTypeResolver.IsLong(value));
        }

        public static bool IsUnknown(Value value)
        {
            return value is UndefinedValue
                || value is AnyValue
                || value is IntegerIntervalValue
                || value is LongintIntervalValue
                || value is FloatIntervalValue;
        }
    }
}
