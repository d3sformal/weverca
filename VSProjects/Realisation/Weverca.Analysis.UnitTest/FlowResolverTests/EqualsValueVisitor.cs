using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest.FlowResolverTests
{
    internal class EqualsValueVisitor : AbstractValueVisitor
    {
        private Value expectedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsValueVisitor" /> class.
        /// </summary>
        /// <param name="expectedValue"></param>
        public EqualsValueVisitor(Value expectedValue)
        {
            this.expectedValue = expectedValue;
        }

        #region AbstractValueVisitor Members

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Thrown always</exception>
        public override void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void VisitAnyValue(AnyValue value)
        {
            Assert.IsNotNull(expectedValue as AnyValue);
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            Assert.IsNotNull(expectedValue as UndefinedValue);
        }

        /// <inheritdoc />
        public override void VisitAnyStringValue(AnyStringValue value)
        {
            Assert.IsNotNull(expectedValue as AnyStringValue);
        }

        #region Scalar values

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            var expected = expectedValue as FloatValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        /// <inheritdoc />
        public override void VisitBooleanValue(BooleanValue value)
        {
            var expected = expectedValue as BooleanValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        /// <inheritdoc />
        public override void VisitStringValue(StringValue value)
        {
            StringValue expected = expectedValue as StringValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            var expected = expectedValue as LongintValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            var expected = expectedValue as IntegerValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        #endregion Scalar values

        #region Interval values

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            var expected = expectedValue as IntegerIntervalValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Start, value.Start);
            Assert.AreEqual(expected.End, value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            var expected = expectedValue as LongintIntervalValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Start, value.Start);
            Assert.AreEqual(expected.End, value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            var expected = expectedValue as FloatIntervalValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Start, value.Start);
            Assert.AreEqual(expected.End, value.End);
        }

        #endregion Interval values

        #endregion AbstractValueVisitor Members
    }
}
