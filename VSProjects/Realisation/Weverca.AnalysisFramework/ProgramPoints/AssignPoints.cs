using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;

namespace Weverca.AnalysisFramework.ProgramPoints
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
    /// Assign expression representation
    /// </summary>
    public class AssignConcatPoint : RValuePoint
    {
        public readonly ValueAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        public Operations Operation { get { return Assign.PublicOperation; } }

        /// <summary>
        /// Here will be stored assigned value
        /// </summary>
        public readonly AssignProvider AssignTarget;

        /// <summary>
        /// Assigned operand
        /// </summary>
        public readonly RValuePoint LOperand;

        /// <summary>
        /// Value provider for assign
        /// </summary>
        public readonly RValuePoint ROperand;

        internal AssignConcatPoint(ValueAssignEx assign, RValuePoint lOperand, RValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;

            AssignTarget = lOperand as AssignProvider;
            if (AssignTarget == null)
                throw new NotSupportedException("Given lOperand cannot be used ass assign target");
        }

        protected override void flowThrough()
        {
            RValuePoint firstPart, secondPart;
            switch (Assign.PublicOperation)
            {
                case Operations.AssignPrepend:
                    firstPart = ROperand;
                    secondPart = LOperand;
                    break;
                case Operations.AssignAppend:
                    firstPart = LOperand;
                    secondPart = ROperand;
                    break;
                default:
                    throw new NotSupportedException("Given concat assign is not supported");
            }

            Value = Services.Evaluator.Concat(new MemoryEntry[] { firstPart.Value, secondPart.Value });
            AssignTarget.Assign(Flow, Value);
        }

    }

    /// <summary>
    /// Assign expression representation
    /// </summary>
    public class AssignOperationPoint : RValuePoint
    {
        public readonly ValueAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        public Operations Operation { get { return Assign.PublicOperation; } }

        /// <summary>
        /// Here will be stored assigned value
        /// </summary>
        public readonly AssignProvider AssignTarget;

        /// <summary>
        /// Assigned operand
        /// </summary>
        public readonly RValuePoint LOperand;

        /// <summary>
        /// Value provider for assign
        /// </summary>
        public readonly RValuePoint ROperand;

        internal AssignOperationPoint(ValueAssignEx assign, RValuePoint lOperand, RValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;

            AssignTarget = lOperand as AssignProvider;
            if (AssignTarget == null)
                throw new NotSupportedException("Given lOperand cannot be used ass assign target");
        }

        protected override void flowThrough()
        {
            var binaryOperation = toBinaryOperation(Assign.PublicOperation);
            Value = Services.Evaluator.BinaryEx(LOperand.Value, binaryOperation, ROperand.Value);
            AssignTarget.Assign(Flow, Value);
        }

        private Operations toBinaryOperation(Operations assignOperation)
        {
            switch (assignOperation)
            {
                case Operations.AssignPrepend:
                case Operations.AssignAppend:
                    throw new NotSupportedException("This is not supported assign operation");
                case Operations.AssignAdd:
                    return Operations.Add;
                case Operations.AssignAnd:
                    return Operations.And;
                case Operations.AssignDiv:
                    return Operations.Div;
                case Operations.AssignMod:
                    return Operations.Mod;
                case Operations.AssignMul:
                    return Operations.Mul;
                case Operations.AssignOr:
                    return Operations.Or;
                case Operations.AssignShiftLeft:
                    return Operations.ShiftLeft;
                case Operations.AssignShiftRight:
                    return Operations.ShiftRight;
                case Operations.AssignSub:
                    return Operations.Sub;
                case Operations.AssignXor:
                    return Operations.Xor;
                default:
                    throw new NotImplementedException("This assign action is not implemented");
            }
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
