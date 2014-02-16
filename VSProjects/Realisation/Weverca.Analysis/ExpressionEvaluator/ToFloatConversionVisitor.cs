using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    class ToFloatConversionVisitor : AbstractValueVisitor
    {
        FlowOutputSet valueFactory;

        public FloatValue Result { get; set; }

        internal ToFloatConversionVisitor(FlowOutputSet valueFactory)
        {
            this.valueFactory = valueFactory;
        }
        
        public override void VisitValue(Value value)
        {
            Result = null;
        }

        public override void VisitFloatValue(FloatValue value)
        {
            Result = value;
        }

        public override void VisitIntegerValue(IntegerValue value)
        {
            Result = valueFactory.CreateDouble(value.Value);
        }

        public override void VisitLongintValue(LongintValue value)
        {
            Result = valueFactory.CreateDouble(value.Value);
        }
    }
}
