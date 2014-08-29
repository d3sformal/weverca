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


using PHP.Core.AST;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Evaluates one binary operation with abstract integer value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    public class LeftAnyIntegerOperandVisitor : LeftAnyNumericOperandVisitor<AnyIntegerValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyIntegerOperandVisitor" /> class.
        /// </summary>
        public LeftAnyIntegerOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftAnyIntegerOperandVisitor" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        public LeftAnyIntegerOperandVisitor(FlowController flowController)
            : base(flowController)
        {
        }

        #region AbstractValueVisitor Members

        #region Concrete values

        #region Scalar values

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation,
                TypeConversion.ToInteger(value.Value));
            if (result != null)
            {
                return;
            }

            base.VisitBooleanValue(value);
        }

        #region Numeric values

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value.Value);
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value.Value);
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
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                return;
            }

            int integerValue;
            double floatValue;
            bool isInteger;
            TypeConversion.TryConvertToNumber(value.Value, true, out integerValue,
                out floatValue, out isInteger);

            result = isInteger
                ? ArithmeticOperation.LeftAbstractArithmetic(flow, operation, integerValue)
                : ArithmeticOperation.LeftAbstractArithmetic(flow, operation, floatValue);

            if (result != null)
            {
                return;
            }

            base.VisitStringValue(value);
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            base.VisitObjectValue(value);
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            switch (operation)
            {
                case Operations.BitOr:
                case Operations.BitXor:
                case Operations.ShiftLeft:
                case Operations.ShiftRight:
                    result = leftOperand;
                    break;
                case Operations.Mul:
                    result = OutSet.CreateInt(0);
                    break;
                default:
                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #region Interval values

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value);
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Mod:
                    result = ModuloOperation.AbstractModulo(flow, value);
                    break;
                default:
                    result = ArithmeticOperation.LeftAbstractArithmetic(flow, operation, value);
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

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            result = ArithmeticOperation.RightAbstractBooleanArithmetic(flow, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyBooleanValue(value);
        }

        #region Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            switch (operation)
            {
                case Operations.Identical:
                case Operations.NotIdentical:
                    result = OutSet.AnyBooleanValue;
                    break;
                default:
                    result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyIntegerValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitAnyFloatValue(AnyFloatValue value)
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
                    result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
                    if (result != null)
                    {
                        break;
                    }

                    base.VisitAnyFloatValue(value);
                    break;
            }
        }

        #endregion Abstract numeric values

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            result = ArithmeticOperation.AbstractFloatArithmetic(Snapshot, operation);
            if (result != null)
            {
                // A string can be converted into floating point number too.
                return;
            }

            base.VisitAnyStringValue(value);
        }

        #endregion Abstract scalar values

        #region Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyObjectValue(AnyObjectValue value)
        {
            result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by arithmetic operation",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            base.VisitAnyObjectValue(value);
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = ArithmeticOperation.AbstractIntegerArithmetic(flow, operation);
            if (result != null)
            {
                // Arithmetic with resources is nonsence
                return;
            }

            base.VisitAnyResourceValue(value);
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}