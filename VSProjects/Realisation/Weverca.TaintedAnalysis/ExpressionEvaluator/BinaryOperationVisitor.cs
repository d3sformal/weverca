using System;

using PHP.Core;
using PHP.Core.AST;

using Weverca.Analysis.Memory;

namespace Weverca.TaintedAnalysis.ExpressionEvaluator
{
    internal interface ILeftOperandVisitor : IValueVisitor
    {
    }

    //TODO: Why not to use AbstractValueVisitor ? (e.g exact resolving of function values is not needed for binary operations)
    internal class LeftOperandBooleanValueVisitor : ILeftOperandVisitor
    {
        private BooleanValue leftOperand;
        private BinaryOperationVisitor visitor;

        internal LeftOperandBooleanValueVisitor(BooleanValue value, BinaryOperationVisitor binaryVisitor)
        {
            leftOperand = value;
            visitor = binaryVisitor;
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
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public void VisitStringValue(StringValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException();
        }

        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
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
    }

    //TODO: Why not to use AbstractValueVisitor ? (e.g exact resolving of function values is not needed for binary operations)
    internal class LeftOperandIntegerValueVisitor : ILeftOperandVisitor
    {
        private IntegerValue leftOperand;
        private BinaryOperationVisitor visitor;

        internal LeftOperandIntegerValueVisitor(IntegerValue value, BinaryOperationVisitor binaryVisitor)
        {
            leftOperand = value;
            visitor = binaryVisitor;
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
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public void VisitStringValue(StringValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
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

        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyResourceValue(AnyResourceValue value)
        {
            throw new NotImplementedException();
        }

        #endregion


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
    }

    //TODO: Why not to use AbstractValueVisitor ? (e.g exact resolving of function values is not needed for binary operations)
    internal class LeftOperandFloatValueVisitor : ILeftOperandVisitor
    {
        private FloatValue leftOperand;
        private BinaryOperationVisitor visitor;

        internal LeftOperandFloatValueVisitor(FloatValue value, BinaryOperationVisitor binaryVisitor)
        {
            leftOperand = value;
            visitor = binaryVisitor;
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
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
        }

        public void VisitStringValue(StringValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException();
        }

        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            visitor.Result = BinaryOperations.BinaryOperation(visitor.Evaluator,
                leftOperand, visitor.Operation, value);
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
    }

    //TODO: Why not to use AbstractValueVisitor ? (e.g exact resolving of function values is not needed for binary operations)
    public class BinaryOperationVisitor : IValueVisitor
    {
        private ILeftOperandVisitor visitor;

        public BinaryOperationVisitor(ExpressionEvaluator expressionEvaluator)
        {
            Evaluator = expressionEvaluator;
        }

        public Value Evaluate(Value leftOperand, Operations binaryOperation, Value rightOperand)
        {
            // Sets current operation
            Operation = binaryOperation;

            // Gets type of left operand and creates appropriate visitor
            Debug.Assert(visitor == null, "Visitor must never be null");
            leftOperand.Accept(this);

            // Gets type of right operand and evaluate expression for given operation
            Debug.Assert(visitor != null, "Visitor must never be null");
            rightOperand.Accept(visitor);
            visitor = null;

            // Returns result of binary operation
            Debug.Assert(Result != null, "Visitor of left operand must return value");
            var result = Result;
            Result = null;
            return result;
        }

        internal ExpressionEvaluator Evaluator { get; private set; }

        internal Operations Operation { get; private set; }

        internal Value Result { get; set; }

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
            visitor = new LeftOperandFloatValueVisitor(value, this);
        }

        public void VisitBooleanValue(BooleanValue value)
        {
            visitor = new LeftOperandBooleanValueVisitor(value, this);
        }

        public void VisitStringValue(StringValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitLongintValue(LongintValue value)
        {
            throw new NotSupportedException();
        }

        public void VisitIntegerValue(IntegerValue value)
        {
            visitor = new LeftOperandIntegerValueVisitor(value, this);
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

        public void VisitAnyFloatValue(AnyFloatValue value)
        {
            throw new NotImplementedException();
        }

        public void VisitAnyResourceValue(AnyResourceValue value)
        {
            throw new NotImplementedException();
        }

        #endregion


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
    }
}
