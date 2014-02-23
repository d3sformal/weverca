using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    internal class ToFloatConversionVisitor : AbstractValueVisitor
    {
        ISnapshotReadWrite snapshot;

        public FloatValue Result { get; set; }

        internal ToFloatConversionVisitor(ISnapshotReadWrite snapshotReadWrite)
        {
            snapshot = snapshotReadWrite;
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            Result = null;
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            Result = value;
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            Result = snapshot.CreateDouble(value.Value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            Result = snapshot.CreateDouble(value.Value);
        }
    }
}
