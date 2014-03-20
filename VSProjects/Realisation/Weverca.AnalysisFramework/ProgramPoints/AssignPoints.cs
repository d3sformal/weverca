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
        /// <summary>
        /// Assign element represented by current point
        /// </summary>
        public readonly ValueAssignEx Assign;

        /// <inheritdoc />
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
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Value = ROperand.Value;
            IntegerValue v = Value.ReadMemory(Flow.OutSet.Snapshot).PossibleValues.First() as IntegerValue;
            int vInt;
            if (v != null)
                vInt = v.Value;
            Services.Evaluator.Assign(LOperand.LValue, Value.ReadMemory(OutSnapshot));
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitAssign(this);
        }
    }

    /// <summary>
    /// Assign expression representation
    /// </summary>
    public class AssignConcatPoint : ValuePoint
    {
        /// <summary>
        /// Assign element represented by current point
        /// </summary>
        public readonly ValueAssignEx Assign;

        /// <inheritdoc />
        public override LangElement Partial { get { return Assign; } }

        /// <summary>
        /// Assign operation specifier
        /// </summary>
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
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;

            AssignTarget = lOperand as LValuePoint;
            if (AssignTarget == null)
                throw new NotSupportedException("Given lOperand cannot be used ass assign target");
        }

        /// <inheritdoc />
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

            var concatedValue = Services.Evaluator.Concat(new MemoryEntry[] { firstPart.Value.ReadMemory(OutSnapshot), secondPart.Value.ReadMemory(OutSnapshot) });
            Value = OutSet.CreateSnapshotEntry(concatedValue);
            Services.Evaluator.Assign(AssignTarget.LValue, concatedValue);
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitAssignConcat(this);
        }
    }

    /// <summary>
    /// Assign expression representation
    /// </summary>
    public class AssignOperationPoint : ValuePoint
    {
        /// <summary>
        /// Assign element represented by current point
        /// </summary>
        public readonly ValueAssignEx Assign;

        /// <inheritdoc />
        public override LangElement Partial { get { return Assign; } }

        /// <summary>
        /// Assign operation specifier
        /// </summary>
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
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;

            AssignTarget = lOperand as LValuePoint;
            if (AssignTarget == null)
                throw new NotSupportedException("Given lOperand cannot be used ass assign target");
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            var binaryOperation = toBinaryOperation(Assign.PublicOperation);
            var value= Services.Evaluator.BinaryEx(LOperand.Value.ReadMemory(OutSnapshot), binaryOperation, ROperand.Value.ReadMemory(OutSnapshot));
            Value = OutSet.CreateSnapshotEntry(value);

            Services.Evaluator.Assign(LOperand.LValue, value);
        }

        private Operations toBinaryOperation(Operations assignOperation)
        {
            switch (assignOperation)
            {
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

                case Operations.AssignPrepend:
                case Operations.AssignAppend:
                default:
                    throw new NotSupportedException("Assign operation "+assignOperation+ " is not supported within AssignOperationPoint");
            }
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitAssignOperation(this);
        }
    }

    /// <summary>
    /// Reference assign representation
    /// </summary>
    public class RefAssignPoint : ValuePoint
    {
        /// <summary>
        /// Assign element represented by current point
        /// </summary>
        public readonly RefAssignEx Assign;

        /// <inheritdoc />
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
            LOperand = lOperand;
            ROperand = rOperand;
            Assign = assign;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Services.Evaluator.AliasAssign(LOperand.LValue, ROperand.Value);

            Value=ROperand.Value;
        }

        /// <inheritdoc />
        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitRefAssign(this);
        }
    }
}
