﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.UnitTest.FlowResolverTests
{
    class EqualsValueVisitor : IValueVisitor
    {
        Value expectedValue;

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
            throw new NotImplementedException();
        }

        public void VisitUndefinedValue(UndefinedValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyStringValue(AnyStringValue value)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitStringValue(StringValue value)
        {
            StringValue expected = expectedValue as StringValue;
            Assert.IsNotNull(expectedValue);
            Assert.AreEqual(expected.Value, value.Value);
        }

        public void VisitLongintValue(LongintValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}