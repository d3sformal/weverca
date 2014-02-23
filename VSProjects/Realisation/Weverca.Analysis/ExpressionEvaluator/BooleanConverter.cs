using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts a value to boolean during the analysis.
    /// </summary>
    /// <remarks>
    /// Boolean casting is defined by operations <see cref="Operations.BoolCast" />.
    /// </remarks>
    public class BooleanConverter : AbstractValueVisitor
    {
        /// <summary>
        /// Result of conversion when the value can be converted to a concrete boolean.
        /// </summary>
        private BooleanValue result;

        /// <summary>
        /// Read-write memory snapshot of context used for fix-point analysis.
        /// </summary>
        private SnapshotBase snapshot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanConverter" /> class.
        /// </summary>
        public BooleanConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanConverter" /> class.
        /// </summary>
        /// <param name="snapshotBase">Read-write memory snapshot used for fix-point analysis.</param>
        public BooleanConverter(SnapshotBase snapshotBase)
        {
            snapshot = snapshotBase;
        }

        /// <summary>
        /// Set output set current evaluation context.
        /// </summary>
        /// <remarks>
        /// The flow controller changes for every expression, so it must be always called again.
        /// </remarks>
        /// <param name="snapshotBase">Read-write memory snapshot used for fix-point analysis.</param>
        public void SetContext(SnapshotBase snapshotBase)
        {
            snapshot = snapshotBase;
        }

        #region Boolean conversion

        /// <summary>
        /// Converts the value to boolean, concrete or abstract, depending on success of conversion.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>
        /// Concrete value if the conversion is successful, otherwise <see cref="AnyBooleanValue" />.
        /// </returns>
        public Value Evaluate(Value value)
        {
            // Gets type of value and convert to boolean
            value.Accept(this);

            // Returns result of boolean conversion
            if (result != null)
            {
                return result;
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Converts the value to concrete boolean value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Concrete boolean value if conversion is successful, otherwise <c>null</c>.</returns>
        public BooleanValue EvaluateToBoolean(Value value)
        {
            // Gets type of value and convert to boolean
            value.Accept(this);

            // Returns result of boolean conversion. Null value indicates both true and false
            return result;
        }

        /// <summary>
        /// Converts the value to abstract boolean value.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Abstract boolean value if conversion is successful, otherwise <c>null</c>.</returns>
        public AnyBooleanValue EvaluateToAnyBoolean(Value value)
        {
            // Gets type of value and convert to boolean
            value.Accept(this);

            // Returns result of boolean conversion. Null value indicates that value is concrete boolean
            if (result != null)
            {
                return null;
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Converts all possible values in memory entry to concrete or abstract boolean.
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert.</param>
        /// <returns>
        /// Concrete value if the conversion is successful, otherwise <see cref="AnyBooleanValue" />.
        /// </returns>
        public Value Evaluate(MemoryEntry entry)
        {
            var value = EvaluateToBoolean(entry);

            // Returns result of boolean conversion
            if (value != null)
            {
                return value;
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Converts all possible values in memory entry to concrete boolean value.
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert.</param>
        /// <returns>Concrete boolean value if conversion is successful, otherwise <c>null</c>.</returns>
        public BooleanValue EvaluateToBoolean(MemoryEntry entry)
        {
            var isNotTrue = true;
            var isNotFalse = true;

            foreach (var value in entry.PossibleValues)
            {
                // Gets type of value and convert to boolean
                value.Accept(this);

                if (result != null)
                {
                    if (result.Value)
                    {
                        if (isNotTrue)
                        {
                            isNotTrue = false;
                        }
                    }
                    else
                    {
                        if (isNotFalse)
                        {
                            isNotFalse = false;
                        }
                    }
                }
                else
                {
                    if (isNotTrue)
                    {
                        isNotTrue = false;
                    }

                    if (isNotFalse)
                    {
                        isNotFalse = false;
                    }
                }
            }

            // Returns result of boolean conversion. Null value indicates both true and false
            if (isNotTrue)
            {
                if (isNotFalse)
                {
                    Debug.Fail("Expression must be possible to convert into boolean value");
                    return null;
                }
                else
                {
                    return snapshot.CreateBool(false);
                }
            }
            else
            {
                if (isNotFalse)
                {
                    return snapshot.CreateBool(true);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Converts all possible values in memory entry to abstract boolean value.
        /// </summary>
        /// <param name="entry">Memory entry with all possible values to convert.</param>
        /// <returns>Abstract boolean value if conversion is successful, otherwise <c>null</c>.</returns>
        public AnyBooleanValue EvaluateToAnyBoolean(MemoryEntry entry)
        {
            var value = EvaluateToBoolean(entry);

            // Returns result of boolean conversion. Null value indicates that value is concrete boolean
            if (value != null)
            {
                return null;
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        #endregion Boolean conversion

        #region Logical operations

        /// <summary>
        /// Convert operands to boolean and perform logical operation.
        /// </summary>
        /// <param name="leftOperand">The left operand of logical operation.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="rightOperand">The right operand of logical operation.</param>
        /// <returns>If operation is logical, result of the operation, otherwise <c>null</c>.</returns>
        public Value EvaluateLogicalOperation(Value leftOperand, Operations operation,
            Value rightOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    return EvaluateAndOperation(leftOperand, rightOperand);
                case Operations.Or:
                    return EvaluateOrOperation(leftOperand, rightOperand);
                case Operations.Xor:
                    return EvaluateAndOperation(leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical AND operation for given operands.
        /// </summary>
        /// <param name="leftOperand">The left operand of AND operation.</param>
        /// <param name="rightOperand">The right operand of AND operation.</param>
        /// <returns>Result of logical AND operation.</returns>
        public Value EvaluateAndOperation(Value leftOperand, Value rightOperand)
        {
            var leftBoolean = EvaluateToBoolean(leftOperand);

            if ((leftBoolean == null) || leftBoolean.Value)
            {
                var rightBoolean = EvaluateToBoolean(rightOperand);

                if ((rightBoolean == null) || rightBoolean.Value)
                {
                    if ((leftBoolean != null) && (rightBoolean != null))
                    {
                        return leftBoolean;
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                }
                else
                {
                    return rightBoolean;
                }
            }
            else
            {
                return leftBoolean;
            }
        }

        /// <summary>
        /// Perform logical OR operation for given operands.
        /// </summary>
        /// <param name="leftOperand">The left operand of OR operation.</param>
        /// <param name="rightOperand">The right operand of OR operation.</param>
        /// <returns>Result of logical OR operation.</returns>
        public Value EvaluateOrOperation(Value leftOperand, Value rightOperand)
        {
            var leftBoolean = EvaluateToBoolean(leftOperand);

            if ((leftBoolean != null) && leftBoolean.Value)
            {
                return leftBoolean;
            }
            else
            {
                var rightBoolean = EvaluateToBoolean(rightOperand);

                if ((rightBoolean != null) && rightBoolean.Value)
                {
                    return rightBoolean;
                }
                else
                {
                    if ((leftBoolean != null) && (rightBoolean != null))
                    {
                        return leftBoolean;
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                }
            }
        }

        /// <summary>
        /// Perform logical XOR operation for given operands.
        /// </summary>
        /// <param name="leftOperand">The left operand of XOR operation.</param>
        /// <param name="rightOperand">The right operand of XOR operation.</param>
        /// <returns>Result of logical XOR operation.</returns>
        public Value EvaluateXorOperation(Value leftOperand, Value rightOperand)
        {
            var leftBoolean = EvaluateToBoolean(leftOperand);
            var rightBoolean = EvaluateToBoolean(rightOperand);

            if ((leftBoolean != null) && (rightBoolean != null))
            {
                if (leftBoolean.Value)
                {
                    if (rightBoolean.Value)
                    {
                        return snapshot.CreateBool(false);
                    }
                    else
                    {
                        return leftBoolean;
                    }
                }
                else
                {
                    return rightBoolean;
                }
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        /// <summary>
        /// Convert all values in memory entries to boolean and perform logical operation.
        /// </summary>
        /// <param name="leftOperand">The left operand of logical operation.</param>
        /// <param name="operation">Operation to be performed, only the logical one gives a result.</param>
        /// <param name="rightOperand">The right operand of logical operation.</param>
        /// <returns>If operation is logical, result of the operation, otherwise <c>null</c>.</returns>
        public Value EvaluateLogicalOperation(MemoryEntry leftOperand, Operations operation,
            MemoryEntry rightOperand)
        {
            switch (operation)
            {
                case Operations.And:
                    return EvaluateAndOperation(leftOperand, rightOperand);
                case Operations.Or:
                    return EvaluateOrOperation(leftOperand, rightOperand);
                case Operations.Xor:
                    return EvaluateAndOperation(leftOperand, rightOperand);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Perform logical AND operation for all combinations values in memory entries.
        /// </summary>
        /// <param name="leftOperand">The left operand of AND operation.</param>
        /// <param name="rightOperand">The right operand of AND operation.</param>
        /// <returns>Result of logical AND operation.</returns>
        public Value EvaluateAndOperation(MemoryEntry leftOperand, MemoryEntry rightOperand)
        {
            var leftBoolean = EvaluateToBoolean(leftOperand);

            if ((leftBoolean == null) || leftBoolean.Value)
            {
                var rightBoolean = EvaluateToBoolean(rightOperand);

                if ((rightBoolean == null) || rightBoolean.Value)
                {
                    if ((leftBoolean != null) && (rightBoolean != null))
                    {
                        return leftBoolean;
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                }
                else
                {
                    return rightBoolean;
                }
            }
            else
            {
                return leftBoolean;
            }
        }

        /// <summary>
        /// Perform logical OR operation for all combinations values in memory entries.
        /// </summary>
        /// <param name="leftOperand">The left operand of OR operation.</param>
        /// <param name="rightOperand">The right operand of OR operation.</param>
        /// <returns>Result of logical OR operation.</returns>
        public Value EvaluateOrOperation(MemoryEntry leftOperand, MemoryEntry rightOperand)
        {
            var leftBoolean = EvaluateToBoolean(leftOperand);

            if ((leftBoolean != null) && leftBoolean.Value)
            {
                return leftBoolean;
            }
            else
            {
                var rightBoolean = EvaluateToBoolean(rightOperand);

                if ((rightBoolean != null) && rightBoolean.Value)
                {
                    return rightBoolean;
                }
                else
                {
                    if ((leftBoolean != null) && (rightBoolean != null))
                    {
                        return leftBoolean;
                    }
                    else
                    {
                        return snapshot.AnyBooleanValue;
                    }
                }
            }
        }

        /// <summary>
        /// Perform logical XOR operation for all combinations values in memory entries.
        /// </summary>
        /// <param name="leftOperand">The left operand of XOR operation.</param>
        /// <param name="rightOperand">The right operand of XOR operation.</param>
        /// <returns>Result of logical XOR operation.</returns>
        public Value EvaluateXorOperation(MemoryEntry leftOperand, MemoryEntry rightOperand)
        {
            var leftBoolean = EvaluateToBoolean(leftOperand);
            var rightBoolean = EvaluateToBoolean(rightOperand);

            if ((leftBoolean != null) && (rightBoolean != null))
            {
                if (leftBoolean.Value)
                {
                    if (rightBoolean.Value)
                    {
                        return snapshot.CreateBool(false);
                    }
                    else
                    {
                        return leftBoolean;
                    }
                }
                else
                {
                    return rightBoolean;
                }
            }
            else
            {
                return snapshot.AnyBooleanValue;
            }
        }

        #endregion Logical operations

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <exception cref="System.NotImplementedException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new NotImplementedException("There is no way to convert value to boolean");
        }

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            result = value;
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitResourceValue(ResourceValue value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            TypeConversion.TryConvertToBoolean(snapshot, value, out result);
        }

        #endregion Interval values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            result = null;
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = null;
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = null;
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = TypeConversion.ToBoolean(snapshot, value);
        }

        #endregion Abstract values

        #region Function values

        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">
        /// Thrown always since function value is not valid in an expression
        /// </exception>
        public override void VisitFunctionValue(FunctionValue value)
        {
            throw new ArgumentException("Expression cannot contain any function value");
        }

        /// <inheritdoc />
        /// <exception cref="System.NotSupportedException">Thrown always</exception>
        public override void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            // TODO: There is no special lambda type, it is implemented as Closure object with __invoke()
            throw new NotSupportedException("Lambda function evaluation is not currently supported");
        }

        #endregion Function values

        #region Type values

        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">Thrown always since type value is not valid</exception>
        public override void VisitTypeValue(TypeValue value)
        {
            throw new ArgumentException("Expression cannot contain any type value");
        }

        #endregion Type values

        #region Special values

        /// <inheritdoc />
        /// <exception cref="System.ArgumentException">
        /// Thrown always since special value is not valid in an expression
        /// </exception>
        public override void VisitSpecialValue(SpecialValue value)
        {
            throw new ArgumentException("Expression cannot contain any special value");
        }

        #endregion Special values

        #endregion AbstractValueVisitor Members
    }
}
