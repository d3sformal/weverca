using System;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts an <see cref="IntervalValue"/> to <see cref="FloatIntervalValue"/>.
    /// Be careful about the conversions from given type to <see cref="float"/>!
    /// </summary>
    internal class ToFloatIntervalConversionVisitor : AbstractValueVisitor
    {
        private ISnapshotReadWrite snapshot;

        public FloatIntervalValue Result { get; set; }

        internal ToFloatIntervalConversionVisitor(ISnapshotReadWrite snapshotReadWrite)
        {
            snapshot = snapshotReadWrite;
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            Result = value;
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            Result = snapshot.CreateFloatInterval(value.Start, value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            Result = snapshot.CreateFloatInterval(value.Start, value.End);
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Not supported value type used.");
        }
    }

    /// <summary>
    /// Converts an <see cref="IntervalValue"/> to <see cref="IntegerIntervalValue"/>.
    /// Be careful about the conversions from given type to <see cref="int"/>!
    /// </summary>
    internal class ToIntegerIntervalConversionVisitor : AbstractValueVisitor
    {
        private ISnapshotReadWrite snapshot;

        public IntegerIntervalValue Result { get; set; }

        internal ToIntegerIntervalConversionVisitor(ISnapshotReadWrite snapshotReadWrite)
        {
            snapshot = snapshotReadWrite;
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            Result = snapshot.CreateIntegerInterval((int)value.Start, (int)value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            Result = value;
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            Result = snapshot.CreateIntegerInterval((int)value.Start, (int)value.End);
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Not supported value type used.");
        }
    }

    /// <summary>
    /// Converts an <see cref="IntervalValue"/> to <see cref="LongintIntervalValue"/>.
    /// Be careful about the conversions from given type to <see cref="long"/>!
    /// </summary>
    internal class ToLongIntervalConversionVisitor : AbstractValueVisitor
    {
        private ISnapshotReadWrite snapshot;

        public LongintIntervalValue Result { get; set; }

        internal ToLongIntervalConversionVisitor(ISnapshotReadWrite snapshotReadWrite)
        {
            snapshot = snapshotReadWrite;
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            Result = snapshot.CreateLongintInterval((long)value.Start, (long)value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            Result = snapshot.CreateLongintInterval(value.Start, value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            Result = value;
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            throw new NotSupportedException("Not supported value type used.");
        }
    }
}
