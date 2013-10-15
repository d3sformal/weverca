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
    public class AssignPoint : ValuePoint
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
        public readonly ValuePoint ROperand;

        internal AssignPoint(ValueAssignEx assign, LValuePoint lOperand, ValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        protected override void flowThrough()
        {
            Value = ROperand.Value;

            Services.Evaluator.Assign(LOperand.LValue, Value.ReadMemory(InSet.Snapshot));
        }
    }

    /// <summary>
    /// Assign expression representation
    /// </summary>
    public class AssignConcatPoint : ValuePoint
    {
        public readonly ValueAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        public Operations Operation { get { return Assign.PublicOperation; } }

        /// <summary>
        /// Here will be stored assigned value
        /// </summary>
        public readonly LValuePoint AssignTarget;

        /// <summary>
        /// Assigned operand
        /// </summary>
        public readonly ValuePoint LOperand;

        /// <summary>
        /// Value provider for assign
        /// </summary>
        public readonly ValuePoint ROperand;

        internal AssignConcatPoint(ValueAssignEx assign, ValuePoint lOperand, ValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;

            AssignTarget = lOperand as LValuePoint;
            if (AssignTarget == null)
                throw new NotSupportedException("Given lOperand cannot be used ass assign target");
        }

        protected override void flowThrough()
        {
            ValuePoint firstPart, secondPart;
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

            var concatedValue = Services.Evaluator.Concat(new MemoryEntry[] { firstPart.Value.ReadMemory(InSnapshot), secondPart.Value.ReadMemory(InSnapshot) });
            Value = OutSet.CreateSnapshotEntry(concatedValue);
            Services.Evaluator.Assign(AssignTarget.LValue, concatedValue);
        }
    }

    /// <summary>
    /// Assign expression representation
    /// </summary>
    public class AssignOperationPoint : ValuePoint
    {
        public readonly ValueAssignEx Assign;

        public override LangElement Partial { get { return Assign; } }

        public Operations Operation { get { return Assign.PublicOperation; } }

        /// <summary>
        /// Here will be stored assigned value
        /// </summary>
        public readonly LValuePoint AssignTarget;

        /// <summary>
        /// Assigned operand
        /// </summary>
        public readonly LValuePoint LOperand;

        /// <summary>
        /// Value provider for assign
        /// </summary>
        public readonly ValuePoint ROperand;

        internal AssignOperationPoint(ValueAssignEx assign, LValuePoint lOperand, ValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;

            AssignTarget = lOperand as LValuePoint;
            if (AssignTarget == null)
                throw new NotSupportedException("Given lOperand cannot be used ass assign target");
        }

        protected override void flowThrough()
        {
            var binaryOperation = toBinaryOperation(Assign.PublicOperation);
            var value= Services.Evaluator.BinaryEx(LOperand.Value.ReadMemory(InSnapshot), binaryOperation, ROperand.Value.ReadMemory(InSnapshot));
            Value = OutSet.CreateSnapshotEntry(value);

            Services.Evaluator.Assign(LOperand.LValue, value);
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
    public class RefAssignPoint : ValuePoint
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
        public readonly ValuePoint ROperand;

        internal RefAssignPoint(RefAssignEx assign, LValuePoint lOperand, ValuePoint rOperand)
        {
            NeedsExpressionEvaluator = true;
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        protected override void flowThrough()
        {
            Services.Evaluator.AliasAssign(LOperand.LValue, ROperand.Value.Aliases(InSet.Snapshot));

            Value=ROperand.Value;
        }
    }
}
