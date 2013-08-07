using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    public class StringConvertVisitor : IValueVisitor
    {
        public string Value { get; private set; }

        #region IValueVisitor Members

        public void VisitValue(Value value)
        {
            throw new NotImplementedException();
        }

        public void VisitObjectValue(ObjectValue value)
        {
            // TODO: Object can by converted only if it has __toString magic method implemented
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
            Value = String.Empty;
        }

        public void VisitAnyStringValue(AnyStringValue value)
        {
            VisitAnyValue(value);
        }

        public void VisitAnyBooleanValue(AnyBooleanValue value)
        {
            VisitAnyValue(value);
        }

        public void VisitAnyIntegerValue(AnyIntegerValue value)
        {
            VisitAnyValue(value);
        }

        public void VisitAnyLongintValue(AnyLongintValue value)
        {
            VisitAnyValue(value);
        }

        public void VisitAnyObjectValue(AnyObjectValue value)
        {
            VisitAnyValue(value);
        }

        public void VisitAnyArrayValue(AnyArrayValue value)
        {
            VisitAnyValue(value);
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
            Value = value.Value.ToString();
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            Value = value.Value ? "1" : String.Empty;
        }

        public void VisitStringValue(StringValue value)
        {
            Value = value.Value;
        }

        public void VisitLongintValue(LongintValue value)
        {
            Value = value.Value.ToString();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            Value = value.Value.ToString();
        }

        public void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            VisitValue(value);
        }

        public void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            VisitGenericIntervalValue<int>(value);
        }

        public void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            VisitGenericIntervalValue<long>(value);
        }

        public void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            VisitGenericIntervalValue<double>(value);
        }

        #endregion
    }
}
