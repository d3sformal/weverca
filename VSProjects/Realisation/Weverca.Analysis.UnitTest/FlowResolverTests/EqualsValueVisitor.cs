using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.UnitTest.FlowResolverTests
{
    // TODO: Why not to use AbstractValueVisitor ? (e.g exact resolving of function values is not needed for binary operations)
    internal class EqualsValueVisitor : IValueVisitor
    {
        private Value expectedValue;

        public EqualsValueVisitor(Value expectedValue)
        {
            this.expectedValue = expectedValue;
        }

        #region IValueVisitor Members

        public void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectValue(ObjectValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAssociativeArray(AssociativeArray value)
        {
            throw new NotImplementedException();
        }

        public void VisitSpecialValue(SpecialValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAliasValue(AliasValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyValue(AnyValue value)
        {
            Assert.IsNotNull(expectedValue as AnyValue);
        }

        public void VisitUndefinedValue(UndefinedValue value)
        {
            Assert.IsNotNull(expectedValue as UndefinedValue);
        }

        public virtual void VisitResourceValue(ResourceValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyPrimitiveValue(AnyPrimitiveValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyStringValue(AnyStringValue value)
        {
            Assert.IsNotNull(expectedValue as AnyStringValue);
        }

        public void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyLongintValue(AnyLongintValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyObjectValue(AnyObjectValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyArrayValue(AnyArrayValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyResourceValue(AnyResourceValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitInfoValue(InfoValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitInfoValue<T>(InfoValue<T> value)
        {
            throw new NotImplementedException();
        }

        public void VisitPrimitiveValue(PrimitiveValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitGenericPrimitiveValue<T>(PrimitiveValue<T> value)
        {
            throw new NotImplementedException();
        }

        public void VisitFunctionValue(FunctionValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitTypeValue(TypeValue typeValue)
        {
            throw new NotImplementedException();
        }

        public void VisitFloatValue(FloatValue value)
        {
            var expected = expectedValue as FloatValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            var expected = expectedValue as BooleanValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        public void VisitStringValue(StringValue value)
        {
            StringValue expected = expectedValue as StringValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        public void VisitLongintValue(LongintValue value)
        {
            var expected = expectedValue as LongintValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            var expected = expectedValue as IntegerValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        public void VisitGenericIntervalValue<T>(IntervalValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            throw new NotImplementedException();
        }

        public void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            var expected = expectedValue as IntegerIntervalValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Start, value.Start);
            Assert.AreEqual(expected.End, value.End);
        }

        public void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            var expected = expectedValue as LongintIntervalValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Start, value.Start);
            Assert.AreEqual(expected.End, value.End);
        }

        public void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            var expected = expectedValue as FloatIntervalValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Start, value.Start);
            Assert.AreEqual(expected.End, value.End);
        }

        public void VisitSourceFunctionValue(SourceFunctionValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitSourceMethodValue(SourceMethodValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitNativeAnalyzerValue(NativeAnalyzerValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitSourceTypeValue(SourceTypeValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitNativeTypeValue(NativeTypeValue value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
