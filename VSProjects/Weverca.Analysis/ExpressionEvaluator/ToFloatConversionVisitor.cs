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