using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Provides visitor functionality for Value class and its descendants
    /// If there will be any change in this class AbstractValueVisitor has to be changed with interface
    /// </summary>
    public interface IValueVisitor
    {

        void VisitValue(Value value);

        #region StructuredValues

        void VisitObjectValue(ObjectValue value);

        void VisitAssociativeArray(AssociativeArray value);

        #endregion

        #region Special Values

        void VisitSpecialValue(SpecialValue value);

        void VisitAliasValue(AliasValue value);

        void VisitAnyValue(AnyValue value);

        void VisitUndefinedValue(UndefinedValue value);

        void VisitAnyStringValue(AnyStringValue value);

        void VisitAnyBooleanValue(AnyBooleanValue value);

        void VisitAnyIntegerValue(AnyIntegerValue value);

        void VisitAnyLongintValue(AnyLongintValue value);

        void VisitAnyObjectValue(AnyObjectValue value);

        void VisitAnyArrayValue(AnyArrayValue value);

        void VisitAnyResourceValue(AnyResourceValue value);

        void VisitInfoValue(InfoValue value);

        void VisitInfoValue<T>(InfoValue<T> value);

        #endregion

        #region Primitive Values

        void VisitPrimitiveValue(PrimitiveValue value);

        void VisitGenericPrimitiveValue<T>(PrimitiveValue<T> value);

        void VisitFunctionValue(FunctionValue value);

        void VisitTypeValue(TypeValue typeValue);

        void VisitFloatValue(FloatValue value);

        void VisitBooleanValue(BooleanValue value);

        void VisitStringValue(StringValue value);

        void VisitLongintValue(LongintValue value);

        void VisitAnyFloatValue(AnyFloatValue value);

        void VisitIntegerValue(IntegerValue value);

        #endregion

        #region IntervalValues

        void VisitGenericIntervalValue<T>(IntervalValue<T> value);

        void VisitIntervalIntegerValue(IntegerIntervalValue value);

        void VisitIntervalLongintValue(LongintIntervalValue value);

        void VisitIntervalFloatValue(FloatIntervalValue value);
        #endregion


        
    }
}
