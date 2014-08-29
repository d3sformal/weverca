/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with abstract array value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    public class LeftAnyArrayOperandVisitor : GenericLeftOperandVisitor<AnyArrayValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyArrayOperandVisitor" /> class.
        /// </summary>
        public LeftAnyArrayOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyArrayOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        public LeftAnyArrayOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            if (ArithmeticOperation.IsArithmetic(operation))
            {
                // Ommitted warning message that object cannot be converted to integer
                // TODO: This must be fatal error
                SetWarning("Unsupported operand type: Arithmetic of two arrays");
                result = OutSet.AnyValue;
                return;
            }

            base.VisitValue(value);
        }

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitScalarValue(ScalarValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitScalarValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.ModuloByBooleanValue(flow, value.Value);
                    break;
                default:
                    result = Comparison.LeftAbstractBooleanCompare(OutSet, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation, value.Value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitBooleanValue(value);
                    break;
            }
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitGenericNumericValue<T>(NumericValue<T> value)
        {
            result = Comparison.LeftAlwaysGreater(OutSet, operation);
            if (result != null)
            {
                return;
            }

            VisitGenericScalarValue(value);
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitFloatValue(value);
                    break;
            }
        }

        #endregion Numeric values

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value.Value);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value.Value));
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.RightAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation",
                            AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                        break;
                    }

                    base.VisitObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow,
                        TypeConversion.ToNativeInteger(Snapshot, value));
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToNativeBoolean(Snapshot, value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAssociativeArray(value);
                    break;
            }
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.LessThan:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Equal:
                case Operations.NotEqual:
                case Operations.GreaterThan:
                case Operations.LessThanOrEqual:
                case Operations.Or:
                case Operations.Xor:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.BitAnd:
                    result = OutSet.CreateInt(0);
                    break;
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = OutSet.AnyIntegerValue;
                    break;
                case Operations.Div:
                    result = ArithmeticOperation.DivisionByNull(flow);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.ModuloByNull(flow);
                    break;
                default:
                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitGenericIntervalValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation, value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntervalIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation, value);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitIntervalFloatValue(value);
                    break;
            }
        }

        #endregion Interval values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    // Ommitted warning message that object cannot be converted to integer
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
                        break;
                    }

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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = LogicalOperation.AbstractLogical(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyScalarValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.ModuloByAnyBooleanValue(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

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
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyNumericValue(value);
                    break;
            }
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.RightAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        SetWarning("Object cannot be converted to integer by bitwise operation",
                            AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                        break;
                    }

                    base.VisitAnyObjectValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyArrayValue(value);
                    break;
            }
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = Comparison.LeftAlwaysGreater(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    result = LogicalOperation.AbstractLogical(OutSet, operation,
                        TypeConversion.ToBoolean(value));
                    if (result != null)
                    {
                        break;
                    }

                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        // Bitwise operation with resource can give any integer
                        break;
                    }

                    base.VisitAnyResourceValue(value);
                    break;
            }
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}