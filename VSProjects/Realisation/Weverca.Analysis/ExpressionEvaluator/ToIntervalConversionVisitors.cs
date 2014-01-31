using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts an <see cref="IntervalValue"/> to <see cref="FloatIntervalValue"/>.
    /// Be careful about the conversions from given type to <see cref="float"/>!
    /// </summary>
    class ToFloatIntervalConversionVisitor : AbstractValueVisitor
    {
        FlowOutputSet valueFactory;
        
        public FloatIntervalValue Result { get; set; }

        internal ToFloatIntervalConversionVisitor(FlowOutputSet valueFactory)
        {
            this.valueFactory = valueFactory;
        }
        
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            Result = value;
        }

        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            valueFactory.CreateFloatInterval(value.Start, value.End);
        }

        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            valueFactory.CreateFloatInterval(value.Start, value.End);
        }

        public override void VisitValue(Value value)
        { }
    }

    /// <summary>
    /// Converts an <see cref="IntervalValue"/> to <see cref="IntegerIntervalValue"/>.
    /// Be careful about the conversions from given type to <see cref="int"/>!
    /// </summary>
    class ToIntegerIntervalConversionVisitor : AbstractValueVisitor
    {
        FlowOutputSet valueFactory;
        
        public IntegerIntervalValue Result { get; set; }

        internal ToIntegerIntervalConversionVisitor(FlowOutputSet valueFactory)
        {
            this.valueFactory = valueFactory;
        }

        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            valueFactory.CreateIntegerInterval((int)value.Start, (int)value.End);
        }

        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            Result = value;
        }

        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            valueFactory.CreateIntegerInterval((int)value.Start, (int)value.End);
        }

        public override void VisitValue(Value value)
        { }
    }

    /// <summary>
    /// Converts an <see cref="IntervalValue"/> to <see cref="LongintIntervalValue"/>.
    /// Be careful about the conversions from given type to <see cref="long"/>!
    /// </summary>
    class ToLongIntervalConversionVisitor : AbstractValueVisitor
    {
        FlowOutputSet valueFactory;

        public LongintIntervalValue Result { get; set; }

        internal ToLongIntervalConversionVisitor(FlowOutputSet valueFactory)
        {
            this.valueFactory = valueFactory;
        }

        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            valueFactory.CreateLongintInterval((long)value.Start, (long)value.End);
        }

        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            valueFactory.CreateLongintInterval(value.Start, value.End);
        }

        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            Result = value;
        }

        public override void VisitValue(Value value)
        { }
    }
}
