/*
Copyright (c) 2012-2014 David Hauzar and Miroslav Vodolan

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


﻿using System;
using System.Globalization;
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
            Services.Evaluator.Assign(LOperand.LValue, Value.ReadMemory(OutSnapshot));
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitAssign(this);
        }
    }

    /// <summary>
    /// List assign expression. 
    /// Represents expressions of a form list(variables) = array.
    /// 
    /// Example:
    /// $arr = array(1, 2, 3);
    /// list($one, , $three) = $arr;
    /// echo $one; // 1
    /// echo $three; // 3
    /// </summary>
    public class AssignListPoint : ValuePoint
    {
        /// <inheritdoc />
        public override LangElement Partial { get { return ListElement; } }

        /// <summary>
        /// List assign element represented by current point
        /// </summary>
        private readonly ListEx ListElement;

        /// <summary>
        /// Left values that are assigned.
        /// </summary>
        private readonly List<LValuePoint> LOperands;

        /// <summary>
        /// Value provider for assign
        /// </summary>
        private readonly ValuePoint ROperand;


        internal AssignListPoint(ListEx listElement, List<LValuePoint> lOperands, ValuePoint rOperand)
        {
            LOperands = lOperands;
            ROperand = rOperand;
            ListElement = listElement;
        }

        /// <inheritdoc />
        protected override void flowThrough()
        {
            Value = ROperand.Value;

            var arrPosition = -1;
            foreach (var lOperand in LOperands) 
            {
                arrPosition++;
                if (lOperand == null) continue; // lOperand to assign not specified in this position

                // Assign to the operand lOperand in position arrPosition
                var arrContentInPosition = ROperand.Value.ReadIndex(OutSnapshot, new MemberIdentifier(System.Convert.ToString(arrPosition, CultureInfo.InvariantCulture)));
                lOperand.LValue.WriteMemory(OutSnapshot, arrContentInPosition.ReadMemory(OutSnapshot));
            }
        }

        internal override void Accept(ProgramPointVisitor visitor)
        {
            visitor.VisitAssignList(this);
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