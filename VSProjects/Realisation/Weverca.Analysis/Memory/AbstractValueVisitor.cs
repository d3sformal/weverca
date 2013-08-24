using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Base class for value visitor.
    /// AbstractValueVisitor has to implement all methods from IValueVisitor interface that every visit method calls visit method for object's supertype. Las't method in hierarchy is abstract.
    /// Allows that visitor implementations doesn't have to implement all methods this methods.
    /// NOTE:
    ///     * Simplifies visitor implementations.
    ///     * Provides functionality for adding new visit methods into IValueVisitor.
    ///     * For implementors - implement VisitValue method and then just methods you need.
    /// </summary>
    public abstract class AbstractValueVisitor : IValueVisitor
    {

        public abstract void VisitValue(Value value);

        /// <summary>
        /// For all values in the memory entry calls their accept functions.
        /// </summary>
        /// <param name="entry">Memory entry to visit.</param>
        public void VisitMemoryEntry(MemoryEntry entry)
        {
            foreach (Value value in entry.PossibleValues)
            {
                value.Accept(this);
            }
        }


        #region Structured Value

        public virtual void VisitObjectValue(ObjectValue value)
        {
            VisitValue(value);
        }

        public virtual void VisitAssociativeArray(AssociativeArray value)
        {
            VisitValue(value);
        }

        #endregion

        #region Special Value

        public virtual void VisitSpecialValue(SpecialValue value)
        {
            VisitValue(value);
        }

        public virtual void VisitAliasValue(AliasValue value)
        {
            VisitSpecialValue(value);
        }

        public virtual void VisitAnyValue(AnyValue value)
        {
            VisitSpecialValue(value);
        }

        public virtual void VisitUndefinedValue(UndefinedValue value)
        {
            VisitSpecialValue(value);
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

        public void VisitAnyFloatValue(AnyFloatValue value)
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

        public void VisitAnyResourceValue(AnyResourceValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitInfoValue(InfoValue value)
        {
            VisitSpecialValue(value);
        }

        public void VisitInfoValue<T>(InfoValue<T> value)
        {
            VisitInfoValue(value);
        }


        #endregion

        #region Primitive Value

        public virtual void VisitPrimitiveValue(PrimitiveValue value)
        {
            VisitValue(value);
        }

        public virtual void VisitGenericPrimitiveValue<T>(PrimitiveValue<T> value)
        {
            VisitPrimitiveValue(value);
        }

        public virtual void VisitFloatValue(FloatValue value)
        {
            VisitGenericPrimitiveValue(value);
        }

        public virtual void VisitBooleanValue(BooleanValue value)
        {
            VisitGenericPrimitiveValue(value);
        }

        public virtual void VisitStringValue(StringValue value)
        {
            VisitGenericPrimitiveValue(value);
        }

        public virtual void VisitLongintValue(LongintValue value)
        {
            VisitGenericPrimitiveValue(value);
        }

        public virtual void VisitIntegerValue(IntegerValue value)
        {
            VisitGenericPrimitiveValue(value);
        }
              
        public virtual void VisitTypeValue(TypeValue typeValue)
        {
            VisitValue(typeValue);
        }

        #endregion

        #region Interval Values

        public void VisitGenericIntervalValue<T>(IntervalValue<T> value)
        {
            VisitValue(value);
        }

        public void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            VisitGenericIntervalValue(value);
        }

        public void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            VisitGenericIntervalValue(value);
        }

        public void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            VisitGenericIntervalValue(value);
        }

        #endregion

        #region Function values

        public virtual void VisitFunctionValue(FunctionValue value)
        {
            VisitValue(value);
        }

        public virtual void VisitSourceFunctionValue(SourceFunctionValue value)
        {
            VisitFunctionValue(value);
        }
        
        public virtual void VisitSourceMethodValue(SourceMethodValue value)
        {
            VisitFunctionValue(value);
        }

        public virtual void VisitNativeAnalyzerValue(NativeAnalyzerValue value)
        {
            VisitFunctionValue(value);
        }

        public virtual void VisitLambdaFunctionValue(LambdaFunctionValue value)
        {
            VisitFunctionValue(value);
        }       

        #endregion


    }
}
