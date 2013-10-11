using System;
using System.Collections.Generic;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one unary operation during the analysis
    /// </summary>
    /// <remarks>
    /// The class can evaluate the following unary operations:
    /// <list type="bullet">
    /// <item><term><see cref="Operations.Plus" /></term></item>
    /// <item><term><see cref="Operations.Minus" /></term></item>
    /// <item><term><see cref="Operations.LogicNegation" /></term></item>
    /// <item><term><see cref="Operations.BitNegation" /></term></item>
    /// <item><term><see cref="Operations.AtSign" /></term></item>
    /// <item><term><see cref="Operations.Print" /></term></item>
    /// <item><term><see cref="Operations.Clone" /></term></item>
    /// <item><term><see cref="Operations.BoolCast" /></term></item>
    /// <item><term><see cref="Operations.Int32Cast" /></term></item>
    /// <item><term><see cref="Operations.DoubleCast" /></term></item>
    /// <item><term><see cref="Operations.FloatCast" /></term></item>
    /// <item><term><see cref="Operations.ObjectCast" /></term></item>
    /// <item><term><see cref="Operations.ArrayCast" /></term></item>
    /// <item><term><see cref="Operations.UnsetCast" /></term></item>
    /// </list>
    /// Some unary operation are not supported because they are not yet supported in PHP
    /// <list type="bullet">
    /// <item><term><see cref="Operations.Int8Cast" /></term></item>
    /// <item><term><see cref="Operations.Int16Cast" /></term></item>
    /// <item><term><see cref="Operations.Int64Cast" /></term></item>
    /// <item><term><see cref="Operations.UInt8Cast" /></term></item>
    /// <item><term><see cref="Operations.UInt16Cast" /></term></item>
    /// <item><term><see cref="Operations.UInt32Cast" /></term></item>
    /// <item><term><see cref="Operations.UInt64Cast" /></term></item>
    /// <item><term><see cref="Operations.DecimalCast" /></term></item>
    /// <item><term><see cref="Operations.BinaryCast" /></term></item>
    /// </list>
    /// Conversion to string provides <see cref="StringConverter" /> for these operations:
    /// <list type="bullet">
    /// <item><term><see cref="Operations.StringCast" /></term></item>
    /// <item><term><see cref="Operations.UnicodeCast" /></term></item>
    /// </list>
    /// </remarks>
    public class UnaryOperationEvaluator : AbstractValueVisitor
    {
        /// <summary>
        /// Flow controller of program point providing data for evaluation (output set, position etc.)
        /// </summary>
        private FlowController flow;

        /// <summary>
        /// String converter used for casting values to string
        /// </summary>
        private StringConverter converter;

        /// <summary>
        /// Unary operation that determines the proper action with operand
        /// </summary>
        private Operations operation;

        /// <summary>
        /// Result of performing the unary operation on the given value
        /// </summary>
        private Value result;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnaryOperationEvaluator" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point</param>
        /// <param name="stringConverter">Converter for string casting</param>
        public UnaryOperationEvaluator(FlowController flowController, StringConverter stringConverter)
        {
            converter = stringConverter;
            SetContext(flowController);
        }

        /// <summary>
        /// Gets output set of a program point
        /// </summary>
        public FlowOutputSet OutSet
        {
            get { return flow.OutSet; }
        }

        /// <summary>
        /// Evaluates the unary operation of the value
        /// </summary>
        /// <param name="unaryOperation">Unary operation to be performed</param>
        /// <param name="operand">One operand of unary operation</param>
        /// <returns>Result of performing the unary operation on the operand</returns>
        public Value Evaluate(Operations unaryOperation, Value operand)
        {
            if ((unaryOperation == Operations.StringCast)
                || (unaryOperation == Operations.UnicodeCast))
            {
                var stringValue = converter.Evaluate(operand);
                if (stringValue != null)
                {
                    return stringValue;
                }
                else
                {
                    return OutSet.AnyStringValue;
                }
            }

            // Sets current operation
            operation = unaryOperation;

            // Gets type of operand and evaluate expression for given operation
            result = null;
            operand.Accept(this);

            // Returns result of unary operation
            Debug.Assert(result != null, "The result must be assigned after visiting the value");
            return result;
        }

        /// <summary>
        /// Evaluates the unary operation of all possible values in memory entry
        /// </summary>
        /// <param name="unaryOperation">Unary operation to be performed</param>
        /// <param name="entry">Memory entry with all possible operands of unary operation</param>
        /// <returns>Resulting entry after performing the unary operation on all possible operands</returns>
        public MemoryEntry Evaluate(Operations unaryOperation, MemoryEntry entry)
        {
            var values = new HashSet<Value>();

            foreach (var value in entry.PossibleValues)
            {
                var result = Evaluate(unaryOperation, value);
                values.Add(result);
            }

            return new MemoryEntry(values);
        }

        /// <summary>
        /// Set current evaluation context.
        /// </summary>
        /// <param name="flowController">Flow controller of program point available for evaluation</param>
        public void SetContext(FlowController flowController)
        {
            flow = flowController;
            converter.SetContext(flowController);
        }

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="message">Message of the warning</param>
        private void SetWarning(string message)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        /// <summary>
        /// Report a warning for the position of current expression
        /// </summary>
        /// <param name="message">Message of the warning</param>
        /// <param name="cause">Cause of the warning</param>
        private void SetWarning(string message, AnalysisWarningCause cause)
        {
            var warning = new AnalysisWarning(message, flow.CurrentPartial, cause);
            AnalysisWarningHandler.SetWarning(OutSet, warning);
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <remarks>
        /// The method performs all operations that have general behavior or are undefined.
        /// Since it is the last visitor method in the hierarchy, it can detect invalid operation.
        /// </remarks>
        /// <exception cref="NotSupportedException">Thrown when operation is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when operation is not unary</exception>
        public override void VisitValue(Value value)
        {
            switch (operation)
            {
                case Operations.UnsetCast:
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
                    SetWarning("Try to suppress a warning of the expression");
                    result = value;
                    break;
                default:
                    throw new InvalidOperationException("Resolving of non-unary operation");
            }
        }

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
        {
            if (!PerformUsualOperation(value))
            {
                base.VisitScalarValue(value);
            }
        }

        /// <inheritdoc />
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
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Bit negation of boolean value");
                    result = OutSet.AnyValue;
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

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            switch (operation)
            {
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value));
                    break;
                default:
                    base.VisitGenericNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    // Result of arithmetic negation can overflow
                    if ((value.Value == 0) || ((-value.Value) != 0))
                    {
                        result = OutSet.CreateInt(-value.Value);
                    }
                    else
                    {
                        // If the number has the lowest value (the most important bit is 1, others are 0
                        // in binary), arithmetic negation of it is zero. PHP behaves differently.
                        // It converts the number to the same positive value, but that cause overflow.
                        // Then integer value is converted to appropriate float value
                        result = OutSet.CreateDouble(-TypeConversion.ToFloat(value.Value));
                    }
                    break;
                case Operations.BitNegation:
                    result = OutSet.CreateInt(~value.Value);
                    break;
                case Operations.Int32Cast:
                    result = value;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    result = OutSet.CreateDouble(value.Value);
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

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    // Result of arithmetic negation can overflow
                    if ((value.Value == 0) || ((-value.Value) != 0))
                    {
                        result = OutSet.CreateLong(-value.Value);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationEvaluator.VisitIntegerValue"/>
                        result = OutSet.CreateDouble(-(TypeConversion.ToFloat(value.Value)));
                    }
                    break;
                case Operations.BitNegation:
                    result = OutSet.CreateLong(~value.Value);
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

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.Minus:
                    result = OutSet.CreateDouble(-value.Value);
                    break;
                case Operations.BitNegation:
                    int nativeIntegerValue;
                    if (TypeConversion.TryConvertToInteger(value.Value, out nativeIntegerValue))
                    {
                        result = OutSet.CreateInt(~nativeIntegerValue);
                    }
                    else
                    {
                        result = OutSet.AnyIntegerValue;
                    }
                    break;
                case Operations.Int32Cast:
                    IntegerValue integerValue;
                    if (TypeConversion.TryConvertToInteger(OutSet, value, out integerValue))
                    {
                        result = integerValue;
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

        #endregion

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            int integerValue;
            double floatValue;
            bool isInteger;

            switch (operation)
            {
                case Operations.Plus:
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);
                    if (isInteger)
                    {
                        result = OutSet.CreateInt(integerValue);
                    }
                    else
                    {
                        result = OutSet.CreateDouble(floatValue);
                    }
                    break;
                case Operations.Minus:
                    TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                        out floatValue, out isInteger);
                    if (isInteger)
                    {
                        if ((integerValue == 0) || ((-integerValue) != 0))
                        {
                            result = OutSet.CreateInt(-integerValue);
                        }
                        else
                        {
                            // <seealso cref="UnaryOperationEvaluator.VisitIntegerValue"/>
                            result = OutSet.CreateDouble(-TypeConversion.ToFloat(integerValue));
                        }
                    }
                    else
                    {
                        result = OutSet.CreateDouble(floatValue);
                    }
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value.Value));
                    break;
                case Operations.BitNegation:
                    // Bit negation is defined for every character, not for the entire string
                    // TODO: Implement. PHP string is stored as array of bytes, but printed in UTF8 encoding
                    result = OutSet.AnyStringValue;
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
                    result = value;
                    break;
                default:
                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion

        #region Compound values

        /// <inheritdoc />
        public override void VisitCompoundValue(CompoundValue value)
        {
            if (operation == Operations.BitNegation)
            {
                // TODO: This must be fatal error
                SetWarning("Unsupported operand types: Bit negation of compound value");
                result = OutSet.AnyValue;
            }
            else
            {
                base.VisitCompoundValue(value);
            }
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    SetWarning("Object cannot be converted to integer by unary plus operation");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Minus:
                    SetWarning("Object cannot be converted to integer by unary minus operation");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value));
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    SetWarning("Object cannot be converted to integer");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    SetWarning("Object cannot be converted to float");
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    result = OutSet.AnyStringValue;
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    break;
                case Operations.Clone:
                    // TODO: Object can by converted only if it has __clone magic method implemented
                    result = OutSet.AnyObjectValue;
                    break;
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

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Unary plus of array");
                    result = OutSet.AnyValue;
                    break;
                case Operations.Minus:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Unary minus of array");
                    result = OutSet.AnyValue;
                    break;
                case Operations.LogicNegation:
                    var booleanValue = TypeConversion.ToBoolean(OutSet, value);
                    result = OutSet.CreateBool(!booleanValue.Value);
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
                    // TODO: This must be fatal error
                    SetWarning("__clone method called on non-object");
                    result = OutSet.AnyValue;
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

        #endregion

        /// <inheritdoc />
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
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value));
                    break;
                case Operations.BitNegation:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Bit negation of resource reference");
                    result = OutSet.AnyValue;
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

        /// <inheritdoc />
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
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value));
                    break;
                case Operations.BitNegation:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Bit negation of null value");
                    result = OutSet.AnyValue;
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

        #endregion

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.LogicNegation:
                    bool nativeBooleanValue;
                    if (TypeConversion.TryConvertToBoolean<T>(value, out nativeBooleanValue))
                    {
                        result = OutSet.CreateBool(!nativeBooleanValue);
                    }
                    else
                    {
                        result = OutSet.AnyBooleanValue;
                    }
                    break;
                case Operations.BoolCast:
                    BooleanValue booleanValue;
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

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            if (value.Start == value.End)
            {
                VisitIntegerValue(OutSet.CreateInt(value.Start));
                return;
            }

            switch (operation)
            {
                case Operations.Minus:
                    // Result of arithmetic negation can overflow
                    if ((value.Start == 0) || ((-value.Start) != 0))
                    {
                        result = OutSet.CreateIntegerInterval(-value.End, -value.Start);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationEvaluator.VisitIntegerValue"/>
                        result = OutSet.CreateFloatInterval(-TypeConversion.ToFloat(value.End),
                            -TypeConversion.ToFloat(value.Start));
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
                    result = OutSet.CreateFloatInterval(TypeConversion.ToFloat(value.Start),
                        TypeConversion.ToFloat(value.End));
                    break;
                default:
                    base.VisitIntervalIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            if (value.Start == value.End)
            {
                VisitLongintValue(OutSet.CreateLong(value.Start));
                return;
            }

            switch (operation)
            {
                case Operations.Minus:
                    // Result of arithmetic negation can overflow
                    if ((value.Start == 0) || ((-value.Start) != 0))
                    {
                        result = OutSet.CreateLongintInterval(-value.End, -value.Start);
                    }
                    else
                    {
                        // <seealso cref="UnaryOperationEvaluator.VisitIntegerValue"/>
                        result = OutSet.CreateFloatInterval(-TypeConversion.ToFloat(value.End),
                            -TypeConversion.ToFloat(value.Start));
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
                    result = OutSet.CreateFloatInterval(TypeConversion.ToFloat(value.Start),
                        TypeConversion.ToFloat(value.End));
                    break;
                default:
                    base.VisitIntervalLongintValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            if (value.Start == value.End)
            {
                VisitFloatValue(OutSet.CreateDouble(value.Start));
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

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                case Operations.Minus:
                    result = OutSet.AnyValue;
                    break;
                case Operations.LogicNegation:
                case Operations.BoolCast:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitNegation:
                    // TODO: This is possible fatal error
                    result = OutSet.AnyValue;
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
                    // TODO: This is possible fatal error
                    result = OutSet.AnyStringValue;
                    break;
                case Operations.Clone:
                    // TODO: This is possible fatal error
                    SetWarning("__clone method called on non-object");
                    result = OutSet.AnyValue;
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    // TODO: This is possible fatal error
                    break;
                case Operations.ObjectCast:
                    result = OutSet.AnyObjectValue;
                    break;
                case Operations.ArrayCast:
                    result = OutSet.AnyArrayValue;
                    break;
                default:
                    base.VisitAnyValue(value);
                    break;
            }
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
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
                        // AnyValue has its own implementation, thou must be skipped
                        base.VisitAnyValue(value);
                    }
                    break;
            }
        }

        /// <inheritdoc />
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
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Bit negation of boolean value");
                    result = OutSet.AnyValue;
                    break;
                default:
                    base.VisitAnyBooleanValue(value);
                    break;
            }
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyNumericValue(AnyNumericValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    result = value;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.AnyBooleanValue;
                    break;
                default:
                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            switch (operation)
            {
                case Operations.Minus:
                    // It can be integer or float
                    result = OutSet.AnyValue;
                    break;
                case Operations.BitNegation:
                    result = value;
                    break;
                default:
                    base.VisitAnyIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyLongintValue(AnyLongintValue value)
        {
            switch (operation)
            {
                case Operations.Minus:
                    // It can be long integer or float
                    result = OutSet.AnyValue;
                    break;
                case Operations.BitNegation:
                    result = value;
                    break;
                default:
                    base.VisitAnyLongintValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
        {
            switch (operation)
            {
                case Operations.Minus:
                    result = value;
                    break;
                case Operations.BitNegation:
                    result = OutSet.AnyIntegerValue;
                    break;
                default:
                    base.VisitAnyFloatValue(value);
                    break;
            }
        }

        #endregion

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    // It can be integer or float
                    result = OutSet.AnyValue;
                    break;
                case Operations.Minus:
                    // It can be integer or float
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

        #endregion

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
        {
            if (operation == Operations.BitNegation)
            {
                // TODO: This must be fatal error
                SetWarning("Unsupported operand types: Bit negation of compound value");
                result = OutSet.AnyValue;
            }
            else
            {
                // AnyValue has its own implementation, thou must be skipped
                base.VisitAnyValue(value);
            }
        }

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    SetWarning("Object cannot be converted to integer by unary plus operation");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Minus:
                    SetWarning("Object cannot be converted to integer by unary minus operation");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value));
                    break;
                case Operations.BoolCast:
                    result = TypeConversion.ToBoolean(OutSet, value);
                    break;
                case Operations.Int32Cast:
                    SetWarning("Object cannot be converted to integer");
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.FloatCast:
                case Operations.DoubleCast:
                    SetWarning("Object cannot be converted to float");
                    result = OutSet.AnyFloatValue;
                    break;
                case Operations.StringCast:
                case Operations.UnicodeCast:
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    result = OutSet.AnyStringValue;
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    // TODO: Object can by converted only if it has __toString magic method implemented
                    break;
                case Operations.Clone:
                    // TODO: Object can by converted only if it has __clone magic method implemented
                    result = OutSet.AnyObjectValue;
                    break;
                case Operations.ObjectCast:
                    result = value;
                    break;
                case Operations.ArrayCast:
                    result = OutSet.AnyArrayValue;
                    break;
                default:
                    base.VisitAnyObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            switch (operation)
            {
                case Operations.Plus:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Unary plus of array");
                    result = OutSet.AnyValue;
                    break;
                case Operations.Minus:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Unary minus of array");
                    result = OutSet.AnyValue;
                    break;
                case Operations.LogicNegation:
                    result = OutSet.AnyBooleanValue;
                    break;
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
                    result = TypeConversion.ToString(OutSet, value);
                    break;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.Clone:
                    // TODO: This must be fatal error
                    SetWarning("__clone method called on non-object");
                    result = OutSet.AnyValue;
                    break;
                case Operations.ObjectCast:
                    result = OutSet.AnyObjectValue;
                    break;
                case Operations.ArrayCast:
                    result = value;
                    break;
                default:
                    base.VisitAnyArrayValue(value);
                    break;
            }
        }

        #endregion

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
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
                    result = OutSet.CreateBool(!TypeConversion.ToBoolean(value));
                    break;
                case Operations.BitNegation:
                    // TODO: This must be fatal error
                    SetWarning("Unsupported operand types: Bit negation of resource reference");
                    result = OutSet.AnyValue;
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
                    result = OutSet.AnyStringValue;
                    break;
                default:
                    if (!PerformUsualOperation(value))
                    {
                        // AnyValue has its own implementation, thou must be skipped
                        base.VisitAnyValue(value);
                    }
                    break;
            }
        }

        #endregion

        #region Function values

        /// <inheritdoc />
        public override void VisitFunctionValue(FunctionValue value)
        {
            throw new ArgumentException("Unary operation is not supported for function type");
        }

        /// <inheritdoc />
        public override void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            // TODO: There is no special lambda type, it is implemented as Closure object with __invoke()
            throw new NotImplementedException();
        }

        #endregion

        #region Type values

        /// <inheritdoc />
        public override void VisitTypeValue(TypeValueBase value)
        {
            throw new ArgumentException("Unary operation is not supported for types");
        }

        #endregion

        #region Special values

        /// <inheritdoc />
        public override void VisitSpecialValue(SpecialValue value)
        {
            throw new ArgumentException("Unary operation is not supported for special values");
        }

        #endregion

        #endregion

        #region Helper methods

        /// <summary>
        /// Performs unary operations that behave in the same way for almost every value types
        /// </summary>
        /// <param name="value">The value which operation will be performed on</param>
        /// <returns><c>true</c> whether the operation is performed, otherwise <c>false</c></returns>
        private bool PerformUsualOperation(Value value)
        {
            switch (operation)
            {
                case Operations.Clone:
                    // TODO: This must be fatal error
                    SetWarning("__clone method called on non-object");
                    result = OutSet.AnyValue;
                    return true;
                case Operations.Print:
                    // The operator convert value to string and print it. The string value is not used
                    // to resolve the entire expression. Instead, the false value is returned.
                    // TODO: This is a quest for tainted analysis
                    result = OutSet.CreateBool(false);
                    return true;
                case Operations.ObjectCast:
                    result = TypeConversion.ToObject(OutSet, value);
                    return true;
                case Operations.ArrayCast:
                    result = TypeConversion.ToArray(OutSet, value);
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
