using System;

using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with fixed scalar value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    /// <typeparam name="TScalar">Type of left scalar operand.</typeparam>
    public abstract class LeftScalarOperandVisitor<TScalar> : GenericLeftOperandVisitor<TScalar>
        where TScalar : ScalarValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftScalarOperandVisitor{TScalar}" /> class.
        /// </summary>
        protected LeftScalarOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftScalarOperandVisitor{TScalar}" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        protected LeftScalarOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.ModuloByBooleanValue(flow, value.Value);
                    break;
                default:
                    base.VisitBooleanValue(value);
                    break;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitCompoundValue(CompoundValue value)
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
                    base.VisitCompoundValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
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
            if (ArithmeticOperation.IsArithmetic(operation))
            {
                // TODO: This must be fatal error
                SetWarning("Unsupported operand type: Arithmetic of array and scalar type");
                result = OutSet.AnyValue;
                return;
            }

            base.VisitAssociativeArray(value);
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.And:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.BitAnd:
                    result = OutSet.CreateInt(0);
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
            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                // It is too complicated to represend result of bitwise operation with interval
                return;
            }

            base.VisitGenericIntervalValue(value);
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
                    result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
                    if (result != null)
                    {
                        // Ommitted warning message that object cannot be converted to integer
                        // Ommitted error report that array is unsupported operand in arithmetic operation
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
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    result = ModuloOperation.ModuloByAnyBooleanValue(flow);
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
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
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
                    base.VisitAnyStringValue(value);
                    break;
            }
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyCompoundValue(AnyCompoundValue value)
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
                    base.VisitAnyCompoundValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            switch (operation)
            {
                case Operations.Mod:
                    SetWarning("Object cannot be converted to integer by modulo operation",
                        AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
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
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow);
                    break;
                default:
                    result = BitwiseOperation.Bitwise(OutSet, operation);
                    if (result != null)
                    {
                        break;
                    }

                    if (ArithmeticOperation.IsArithmetic(operation))
                    {
                        // TODO: This must be fatal error
                        SetWarning("Unsupported operand type: Arithmetic of array and scalar type");
                        result = OutSet.AnyValue;
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
