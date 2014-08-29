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
    /// Evaluates one binary operation with fixed number value as the left operand.
    /// </summary>
    /// <remarks>
    /// Supported binary operations are listed in the <see cref="LeftOperandVisitor" />.
    /// </remarks>
    /// <typeparam name="TComparable">Native type of values in left operand number.</typeparam>
    public abstract class LeftNumericOperandVisitor<TComparable>
        : LeftScalarOperandVisitor<NumericValue<TComparable>>
        where TComparable : IComparable, IComparable<TComparable>, IEquatable<TComparable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeftNumericOperandVisitor{TComparable}" /> class.
        /// </summary>
        protected LeftNumericOperandVisitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeftNumericOperandVisitor{TComparable}" /> class.
        /// </summary>
        /// <param name="flowController">Flow controller of program point.</param>
        protected LeftNumericOperandVisitor(FlowController flowController)
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    base.VisitBooleanValue(value);
                    break;
            }
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
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
                    base.VisitStringValue(value);
                    break;
            }
        }

        #endregion Scalar values

        #region Compound values

        /// <inheritdoc />
        public override void VisitObjectValue(ObjectValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by comparison",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            base.VisitObjectValue(value);
        }

        /// <inheritdoc />
        public override void VisitAssociativeArray(AssociativeArray value)
        {
            result = Comparison.RightAlwaysGreater(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAssociativeArray(value);
        }

        #endregion Compound values

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            // When comparing, both operands are converted to boolean
            switch (operation)
            {
                case Operations.LessThan:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.GreaterThanOrEqual:
                    result = OutSet.CreateBool(true);
                    break;
                case Operations.Add:
                case Operations.Sub:
                    result = leftOperand;
                    break;
                default:
                    base.VisitUndefinedValue(value);
                    break;
            }
        }

        #endregion Concrete values

        #region Abstract values

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                // Ommitted warning message that object cannot be converted to integer
                return;
            }

            base.VisitAnyValue(value);
        }

        #region Abstract scalar values

        /// <inheritdoc />
        public override void VisitAnyScalarValue(AnyScalarValue value)
        {
            result = BitwiseOperation.Bitwise(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyScalarValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyBooleanValue(AnyBooleanValue value)
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
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
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
                case Operations.Identical:
                    result = OutSet.CreateBool(false);
                    break;
                case Operations.NotIdentical:
                    result = OutSet.CreateBool(true);
                    break;
                default:
                    result = Comparison.AbstractCompare(OutSet, operation);
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
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                SetWarning("Object cannot be converted to integer by comparison",
                    AnalysisWarningCause.OBJECT_CONVERTED_TO_INTEGER);
                return;
            }

            base.VisitAnyObjectValue(value);
        }

        /// <inheritdoc />
        public override void VisitAnyArrayValue(AnyArrayValue value)
        {
            result = Comparison.RightAlwaysGreater(OutSet, operation);
            if (result != null)
            {
                return;
            }

            base.VisitAnyArrayValue(value);
        }

        #endregion Abstract compound values

        /// <inheritdoc />
        public override void VisitAnyResourceValue(AnyResourceValue value)
        {
            result = Comparison.AbstractCompare(OutSet, operation);
            if (result != null)
            {
                // Comapring of resource and number makes no sence.
                return;
            }

            base.VisitAnyResourceValue(value);
        }

        #endregion Abstract values

        #endregion AbstractValueVisitor Members
    }
}