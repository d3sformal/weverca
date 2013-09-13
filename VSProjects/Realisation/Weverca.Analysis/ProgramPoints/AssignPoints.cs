using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

namespace Weverca.Analysis.ProgramPoints
{
    /// <summary>
    /// Assign expression representation
    /// </summary>
    public class AssignPoint : RValuePoint
    {
        public readonly ValueAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        /// <summary>
        /// Assigned operand
        /// </summary>
        public readonly LValuePoint LOperand;

        /// <summary>
        /// Value provider for assign
        /// </summary>
        public readonly RValuePoint ROperand;

        internal AssignPoint(ValueAssignEx assign, LValuePoint lOperand, RValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        protected override void flowThrough()
        {
            Value = ROperand.Value;
            LOperand.Assign(Flow, Value);
        }
    }

    /// <summary>
    /// Reference assign representation
    /// </summary>
    public class RefAssignPoint : RValuePoint
    {
        public readonly RefAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        /// <summary>
        /// Assigned operand
        /// </summary>
        public readonly LValuePoint LOperand;

        /// <summary>
        /// Alias value provider
        /// </summary>
        public readonly AliasPoint ROperand;

        internal RefAssignPoint(RefAssignEx assign, LValuePoint lOperand, AliasPoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        protected override void flowThrough()
        {
            LOperand.AssignAlias(Flow, ROperand.Aliases);
        }
    }
}
