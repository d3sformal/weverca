using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHP.Core.AST;

using Weverca.AnalysisFramework.Memory;
using Weverca.AnalysisFramework.Expressions;
using Weverca.AnalysisFramework.ProgramPoints;

namespace Weverca.AnalysisFramework
{
    /// <summary>
    /// TODO flow controller handling
    /// </summary>
    class NextPhaseVisitor : ProgramPointVisitor
    {
        private readonly ExpressionEvaluatorBase _expressions;

        internal NextPhaseVisitor(ExpressionEvaluatorBase expressions)
        {
            _expressions = expressions;
        }

        private SnapshotBase input(ProgramPointBase p)
        {
            return p.InSnapshot;
        }

        private SnapshotBase output(ProgramPointBase p)
        {
            return p.OutSnapshot;
        }

        public override void VisitPoint(ProgramPointBase point)
        {
            throw new NotImplementedException("UnImplemented point: " + point.ToString());
        }

        public override void VisitAssign(AssignPoint p)
        {
            var lValue = p.LOperand.LValue;
            var rValue = p.ROperand.Value.ReadMemory(output(p));
            _expressions.Assign(lValue, rValue);
        }

        public override void VisitAssignConcat(AssignConcatPoint p)
        {
            var lValue = p.AssignTarget.LValue;

            ValuePoint firstPart, secondPart;
            switch (p.Assign.PublicOperation)
            {
                case Operations.AssignPrepend:
                    firstPart = p.ROperand;
                    secondPart = p.LOperand;
                    break;
                case Operations.AssignAppend:
                    firstPart = p.LOperand;
                    secondPart = p.ROperand;
                    break;
                default:
                    throw new NotSupportedException("Given concat assign is not supported");
            }

            var concatedValue = _expressions.Concat(new MemoryEntry[] { 
                firstPart.Value.ReadMemory(input(p)), 
                secondPart.Value.ReadMemory(input(p)) 
            });

            _expressions.Assign(lValue, concatedValue);
        }

    }
}
