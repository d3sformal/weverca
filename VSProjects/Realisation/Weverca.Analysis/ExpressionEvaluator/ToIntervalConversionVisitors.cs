/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


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