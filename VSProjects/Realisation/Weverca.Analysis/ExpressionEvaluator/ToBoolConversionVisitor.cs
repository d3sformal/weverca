using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    class ToBoolConversionVisitor : AbstractValueVisitor
    {
        public Value Result { get; private set; }

        FlowOutputSet valueFactory;

        internal ToBoolConversionVisitor(FlowOutputSet valueFactory)
        {
            this.valueFactory = valueFactory;
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            Result = valueFactory.AnyBooleanValue;
        }

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            Result = valueFactory.AnyBooleanValue;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            Result = valueFactory.AnyBooleanValue;
        }

        #region Scalar values

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            Result = value;
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        #endregion Scalar values

        #region Interval values

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            Result = TypeConversion.ToBoolean(valueFactory, value);
        }

        #endregion Interval values

        #endregion AbstractValueVisitor Members
    }
}
