using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{

    /// <summary>
    /// Gets maximum value of current memory entry
    /// </summary>
    class MaxValueVisitor : AbstractValueVisitor
    {
        private double Max=0;
        private FlowOutputSet OutSet;
        
        /// <summary>
        /// Create new instance of MaxValueVisitor
        /// </summary>
        /// <param name="outSet"></param>
        public MaxValueVisitor(FlowOutputSet outSet)
        {
            OutSet = outSet;
        }

        /// <summary>
        /// Returns maximun double value
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public double Evaluate(MemoryEntry entry)
        {
            foreach (var value in entry.PossibleValues)
            {
                value.Accept(this);
            }
            return Max;
        }

        /// <inheritdoc />
        public override void VisitValue(Value value)
        {
            Max = double.MaxValue;
        }

        /// <inheritdoc />
        public override void VisitUndefinedValue(UndefinedValue value)
        {
            Max = Math.Max(Max, 0);
        }

        /// <inheritdoc />
        public override void VisitIntegerValue(IntegerValue value)
        {
            Max = Math.Max(Max, value.Value);
        }

        /// <inheritdoc />
        public override void VisitLongintValue(LongintValue value)
        {
            Max = Math.Max(Max, value.Value);
        }

        /// <inheritdoc />
        public override void VisitFloatValue(FloatValue value)
        {
            Max = Math.Max(Max, value.Value);
        }

        /// <inheritdoc />
        public override void VisitIntervalIntegerValue(IntegerIntervalValue value)
        {
            Max = Math.Max(Max, value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalLongintValue(LongintIntervalValue value)
        {
            Max = Math.Max(Max, value.End);
        }

        /// <inheritdoc />
        public override void VisitIntervalFloatValue(FloatIntervalValue value)
        {
            Max = Math.Max(Max, value.End);
        }

      

    }
}
